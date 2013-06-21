/***************************************************************************
 * Copyright 2012 by
 * + Christian-Albrechts-University of Kiel
 * + Department of Computer Science
 * + Software Engineering Group
 * and others.
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 ***************************************************************************/

package mapping.code.transformator;

import java.util.Arrays;
import java.util.Collection;
import java.util.Deque;
import java.util.HashMap;
import java.util.LinkedList;
import java.util.List;
import java.util.Map;

import lang.csharp.CSharp4AST;
import mapping.IKDMMapper;
import mapping.KDMElementFactory;
import mapping.code.MoDiscoKDM;
import mapping.code.extern.ExternalDatatypeInfoRepository;
import mapping.code.extern.TypeNotFoundException;
import mapping.code.language.GenericLanguageUnitCache;
import mapping.code.language.NotInCacheException;
import mapping.code.namespace.NamespaceSearcher;
import mapping.code.namespace.NamespaceStack;
import mapping.code.resolver.IdentifierResolver;
import mapping.util.KDMChildHelper;
import mapping.util.KeyStringHelper;
import mapping.util.TreeHelper;

import org.antlr.runtime.tree.CommonTree;
import org.antlr.runtime.tree.Tree;
import org.eclipse.gmt.modisco.omg.kdm.action.ActionElement;
import org.eclipse.gmt.modisco.omg.kdm.action.BlockUnit;
import org.eclipse.gmt.modisco.omg.kdm.action.Calls;
import org.eclipse.gmt.modisco.omg.kdm.code.AbstractCodeElement;
import org.eclipse.gmt.modisco.omg.kdm.code.ArrayType;
import org.eclipse.gmt.modisco.omg.kdm.code.ClassUnit;
import org.eclipse.gmt.modisco.omg.kdm.code.CodeItem;
import org.eclipse.gmt.modisco.omg.kdm.code.CodeModel;
import org.eclipse.gmt.modisco.omg.kdm.code.CompilationUnit;
import org.eclipse.gmt.modisco.omg.kdm.code.ControlElement;
import org.eclipse.gmt.modisco.omg.kdm.code.DataElement;
import org.eclipse.gmt.modisco.omg.kdm.code.Datatype;
import org.eclipse.gmt.modisco.omg.kdm.code.EnumeratedType;
import org.eclipse.gmt.modisco.omg.kdm.code.HasValue;
import org.eclipse.gmt.modisco.omg.kdm.code.Imports;
import org.eclipse.gmt.modisco.omg.kdm.code.InterfaceUnit;
import org.eclipse.gmt.modisco.omg.kdm.code.MethodUnit;
import org.eclipse.gmt.modisco.omg.kdm.code.Module;
import org.eclipse.gmt.modisco.omg.kdm.code.Namespace;
import org.eclipse.gmt.modisco.omg.kdm.code.Signature;
import org.eclipse.gmt.modisco.omg.kdm.source.SourceFactory;
import org.eclipse.gmt.modisco.omg.kdm.source.SourceFile;

import util.ListUtil;
import util.Logger;
import util.Pause;

public class Phase3Transformator extends AbstractTransformator implements IKDMMapper<CompilationUnit> {

	private static final Logger				LOGGER			= new Logger(Phase3Transformator.class);

	private final GenericLanguageUnitCache	genericLanguageUnitCache;

	/**
	 * Saves nested namespaces in correct order.<br>
	 * Since the association start from <code>Namespace</code> to <code>CodeItem</code>s is
	 * unspecified, we do not want to rely on the
	 * implementation of the KDM model (e.g. containment or not). We rather save
	 * the namespace hierarchy of a file by our own.
	 */
	private NamespaceStack					internalNamespaceStack;
	private final NamespaceSearcher			searcher;
	private final IdentifierResolver		identifierResolver;
	private final Namespace					internalGlobalNamespace;
	private final Namespace					externalGlobalNamespace;

	private final CodeModel					externalCodeModel;

	private final TypeTransformator			typeTransformator;

	private final LiteralTransformer		literalTransformer;

	private final OperatorTransformator		operatorTransformer;

	private final DeclaratorTransformer		declaratorTransformer;

	private final Deque<List<CodeItem>>		declarations	= new LinkedList<List<CodeItem>>();

	private CompilationUnit					compilationUnit;

	private final CodeModel					internalCodeModel;

	private final Map<String, Imports>		aliases			= new HashMap<String, Imports>();

	private final IdentifierTransformator	identifierTransformator;

	private final LoopTransformator			loopTransformator;

	private final ModifiersTransformator	modifiersTransformator;

	public Phase3Transformator(final GenericLanguageUnitCache genericLanguageUnitCache, final CodeModel internalCodeModel,
			final Module valueRepository, final CodeModel externalCodeModel,
			final ExternalDatatypeInfoRepository externalDatatypeInfoRepository) {
		this.genericLanguageUnitCache = genericLanguageUnitCache;
		this.internalCodeModel = internalCodeModel;
		this.externalCodeModel = externalCodeModel;

		this.internalGlobalNamespace = NamespaceSearcher.getGlobalNamespaceFrom(internalCodeModel);
		this.externalGlobalNamespace = NamespaceSearcher.getGlobalNamespaceFrom(externalCodeModel);

		this.searcher = new NamespaceSearcher(internalGlobalNamespace, externalGlobalNamespace);
		this.identifierResolver = new IdentifierResolver(searcher, externalDatatypeInfoRepository, aliases,
				externalCodeModel);
		this.typeTransformator = new TypeTransformator(identifierResolver, genericLanguageUnitCache, this);
		this.literalTransformer = new LiteralTransformer(genericLanguageUnitCache, valueRepository);
		this.operatorTransformer = new OperatorTransformator(this, identifierResolver);
		this.modifiersTransformator = new ModifiersTransformator();
		this.declaratorTransformer = new DeclaratorTransformer(this, modifiersTransformator, externalCodeModel);
		this.identifierTransformator = new IdentifierTransformator(this);
		this.loopTransformator = new LoopTransformator(this);
	}

	@Override
	public List<CompilationUnit> getMappingResult() {
		return Arrays.asList(compilationUnit);
	}

	@Override
	public void transform(final CommonTree node, final SourceFile sourceFile) {
		LOGGER.info("Start transforming...");
		this.internalNamespaceStack = new NamespaceStack(internalGlobalNamespace, searcher, identifierResolver);
		this.compilationUnit = KDMChildHelper.getCompilationUnit(sourceFile, internalCodeModel);

		try {
			for (int i = 0; i < node.getChildCount(); i++) {
				walk(node.getChild(i), internalGlobalNamespace);
			}
		} catch (RuntimeException e) {
			// additionally display SourceFile upon exception
			System.err.println("RuntimeException in " + sourceFile.getPath());
			throw e;
		}

		LOGGER.info("done.");
	}

	@Override
	@SuppressWarnings("unchecked")
	protected Object walk(final Tree node, final CodeItem parent) throws TypeNotFoundException {
		// an optional node that is not present in the current context, is null
		if (node == null) {
			return null;
		}

		CommonTree commonTreeNode = (CommonTree) node;
		LOGGER.info("Type: " + commonTreeNode.getType());
		LOGGER.info("Text: " + commonTreeNode.getText());
		LOGGER.info("Children: " + commonTreeNode.getChildren());

		Object result;

		switch (commonTreeNode.getType()) {
			case CSharp4AST.NAMESPACE:
				CommonTree qid = (CommonTree) commonTreeNode.getFirstChildWithType(CSharp4AST.QUALIFIED_IDENTIFIER);
				List<String> identifiers = TreeHelper.treeListToStringList(qid.getChildren());

				Namespace namespace = internalNamespaceStack.updateFromCurrentNamespace(identifiers);

				walk(commonTreeNode.getFirstChildWithType(CSharp4AST.NAMESPACE_MEMBER_DECLARATIONS), namespace);

				internalNamespaceStack.popCurrentNamespace(identifiers.size());
				break;
			case CSharp4AST.QUALIFIED_IDENTIFIER:
			case CSharp4AST.ATTRIBUTES:
			case CSharp4AST.TYPE_PARAMETERS:
			case CSharp4AST.NAMESPACE_MEMBER_DECLARATIONS:
			case CSharp4AST.CLASS_MEMBER_DECLARATIONS:
			case CSharp4AST.INTERFACE_MEMBER_DECLARATIONS:
			case CSharp4AST.CONSTANT_DECLARATORS:
			case CSharp4AST.STRUCT_MEMBER_DECLARATIONS:
				for (int i = 0; i < commonTreeNode.getChildCount(); i++) {
					try {
						walk(commonTreeNode.getChild(i), parent);
					} catch (IllegalStateException e) {
						// FIXME for now, ensure transformation of SharpDevelop
						// occurred: Exception in thread "main"
						// java.lang.IllegalStateException: Could not find
						// GrammarData
						// at
						// mapping.util.KDMChildHelper.getAbstractCodeElementByName(KDMChildHelper.java:153)
					}
				}
				break;
			case CSharp4AST.VARIABLE_INITIALIZER:
			case CSharp4AST.CONSTANT_INITIALIZER:
			case CSharp4AST.LOCAL_VARIABLE_INITIALIZER:
			case CSharp4AST.ENUM_MEMBER_INITIALIZER:
				try {
					return walk(node.getChild(0), parent, AbstractCodeElement.class);
				} catch (ExpressionNotSupported e) {
					// skip
				}
				break;
			case CSharp4AST.BLOCK:
				declarations.push(ListUtil.<CodeItem> newNotNullLinkedList());
				BlockUnit blockUnit = KDMElementFactory.createBlockUnit();
				for (int i = 0; i < node.getChildCount(); i++) {
					try {
						AbstractCodeElement blockStmt = walk(node.getChild(i), parent, AbstractCodeElement.class);
						if (blockStmt == null || !(blockStmt instanceof ActionElement)) {
							throw new ExpressionNotSupported("LOGGER.unsupported blockStmt is not an ActionElement");
						}
						blockUnit.getCodeElement().add(blockStmt);
					} catch (TypeNotFoundException e) {
						// skip
						LOGGER.unsupported("(TypeNotFoundException) '" + node.getText() + "'\n\tdue to "
								+ e.getLocalizedMessage());
						// throw e; // test
					} catch (ExpressionNotSupported e) {
						// skip
						LOGGER.unsupported("(ExpressionNotSupported) '" + node.getText() + "'\n\tdue to "
								+ e.getLocalizedMessage());
						// throw e; // test
					} catch (RuntimeException e) {
						// FIXME for now, ensure transformation for SharpDevelop
						LOGGER.unsupported("(RuntimeException) '" + node.getText() + "'\n\tdue to "
								+ e.getLocalizedMessage());
					}
				}
				declarations.pop();
				return blockUnit;
			case CSharp4AST.LOOP_BODY:
			case CSharp4AST.CONDITION:
			case CSharp4AST.THEN:
			case CSharp4AST.ELSE:
			case CSharp4AST.FOR_INITIALIZER:
			case CSharp4AST.FOR_ITERATOR:
				return walk(node.getChild(0), parent);
			case CSharp4AST.EXTERN:
				// TODO
				LOGGER.unsupported("'extern' directive is not yet supported");
				break;
			case CSharp4AST.CLASS:
				declarations.push(ListUtil.<CodeItem> newNotNullLinkedList());
				String className = walk(commonTreeNode.getFirstChildWithType(CSharp4AST.IDENTIFIER), null, String.class);
				ClassUnit classUnit = KDMChildHelper.getAbstractCodeElementByName(compilationUnit.getCodeElement(),
						className, ClassUnit.class);

				walk(commonTreeNode.getFirstChildWithType(CSharp4AST.CLASS_MEMBER_DECLARATIONS), classUnit);

				declarations.pop();
				return classUnit;
			case CSharp4AST.STRUCT:
				declarations.push(ListUtil.<CodeItem> newNotNullLinkedList());
				String structName = commonTreeNode.getFirstChildWithType(CSharp4AST.IDENTIFIER).getText();
				ClassUnit structType = KDMChildHelper.getAbstractCodeElementByName(compilationUnit.getCodeElement(),
						structName, ClassUnit.class);
				structType.setName(structType.getName());
				// TYPE_PARAMETERS
				// IMPLEMENTS
				// TYPE_PARAMETER_CONSTRAINTS_CLAUSES

				// STRUCT_MEMBER_DECLARATIONS
				walk(commonTreeNode.getFirstChildWithType(CSharp4AST.STRUCT_MEMBER_DECLARATIONS), structType);

				LOGGER.unsupported("struct is not yet supported");
				declarations.pop();
				return structType;
			case CSharp4AST.INTERFACE:
				declarations.push(ListUtil.<CodeItem> newNotNullLinkedList());
				String interfaceName = commonTreeNode.getFirstChildWithType(CSharp4AST.IDENTIFIER).getText();
				InterfaceUnit interfaceUnit = KDMChildHelper.getAbstractCodeElementByName(
						compilationUnit.getCodeElement(), interfaceName, InterfaceUnit.class);

				walk(commonTreeNode.getFirstChildWithType(CSharp4AST.INTERFACE_MEMBER_DECLARATIONS), interfaceUnit);

				declarations.pop();
				break;
			case CSharp4AST.ENUM:
				declarations.push(ListUtil.<CodeItem> newNotNullLinkedList());
				String enumName = commonTreeNode.getFirstChildWithType(CSharp4AST.IDENTIFIER).getText();
				EnumeratedType enumeratedType = KDMChildHelper.getAbstractCodeElementByName(
						compilationUnit.getCodeElement(), enumName, EnumeratedType.class);

				// initializers
				// commonTreeNode.getFirstChildWithType(CSharp4AST.ENUM_MEMBER_DECLARATIONS)

				declarations.pop();
				return enumeratedType;
			case CSharp4AST.DELEGATE:
				LOGGER.unsupported("DELEGATE is not yet supported.");
				break;
			case CSharp4AST.FIELD_DECL:
				// do not process children of type TYPE
				for (int i = 0; i < commonTreeNode.getChildCount(); i++) {
					Tree child = commonTreeNode.getChild(i);
					if (child.getType() == CSharp4AST.VARIABLE_DECLARATOR) {
						walk(child, parent);
					}
				}
				break;
			case CSharp4AST.PROPERTY_DECL:
				String propertyName = walk(commonTreeNode.getFirstChildWithType(CSharp4AST.MEMBER_NAME), parent,
						String.class);
				DataElement propertyDecl = KDMChildHelper.getChildItemByName(propertyName, parent, DataElement.class);
				if (propertyDecl == null) {
					LOGGER.fatal(propertyName);
					LOGGER.fatal(KDMChildHelper.getChildrenFromCodeItem(parent));
				}
				declarations.peek().add(propertyDecl);

				LOGGER.unsupported("PROPERTY_DECL's body is not yet supported.");
				break;
			case CSharp4AST.METHOD_DECL:
				declarations.push(ListUtil.<CodeItem> newNotNullLinkedList());
				String methodName = walk(commonTreeNode.getFirstChildWithType(CSharp4AST.MEMBER_NAME), null,
						String.class);

				// walk(commonTreeNode.getFirstChildWithType(CSharp4AST.TYPE_PARAMETERS),
				// null,
				// List.class);

				List<String> formalParameters = walk(
						commonTreeNode.getFirstChildWithType(CSharp4AST.FORMAL_PARAMETER_LIST), null, List.class);

				String keyIdentifier = KeyStringHelper.getMethodKey(methodName, formalParameters);
				MethodUnit methodUnit = KDMChildHelper.getChildMethodByName(keyIdentifier, parent);
				// FIXME if (generic type) parameters are not supported, method is not found
				if (methodUnit == null) {
					LOGGER.warning("Cannot find method with signature: "+keyIdentifier);
					return null;
				}
				Signature signature = (Signature) methodUnit.getCodeElement().get(0);
				declarations.peek().addAll(signature.getParameterUnit());

				BlockUnit methodBody = walk(commonTreeNode.getFirstChildWithType(CSharp4AST.BLOCK), methodUnit,
						BlockUnit.class);
				if (methodBody == null) {
					methodBody = KDMElementFactory.createBlockUnit();
					methodBody.setName("Unsupported method body");
				}
				methodBody.setKind("method body");
				methodUnit.getCodeElement().add(methodBody);

				declarations.pop();
				return methodUnit;
			case CSharp4AST.MEMBER_NAME:
				List<String> typeName = walk(commonTreeNode.getFirstChildWithType(CSharp4AST.NAMESPACE_OR_TYPE_NAME),
						parent, List.class);

				// limited support TODO
				if (typeName.size() > 1) {
					LOGGER.unsupported("member_name with more than one IDENTIFIER is not yet supported: " + typeName);
				}

				String memberName = typeName.get(typeName.size() - 1);
				return memberName;
			case CSharp4AST.FORMAL_PARAMETER_LIST:
				List<String> parameters = new LinkedList<String>();
				for (int i = 0; i < commonTreeNode.getChildCount(); i++) {
					Tree child = commonTreeNode.getChild(i);
					String parameter;
					try {
						parameter = walk(child, parent, String.class);
					} catch (RuntimeException e) {
						System.err.println("Exception in FORMAL_PARAMETER_LIST: " + e.getLocalizedMessage());
						Pause.pause();
						throw e;
					}
					parameters.add(parameter);
				}
				return parameters;
			case CSharp4AST.FIXED_PARAMETER:
			case CSharp4AST.PARAMETER_ARRAY:
				String paramName = getTypeName(commonTreeNode.getFirstChildWithType(CSharp4AST.TYPE));
				return paramName;
			case CSharp4AST.NAMESPACE_OR_TYPE_NAME:
				return identifierTransformator.transformNamespaceOrTypename(commonTreeNode, parent);
			case CSharp4AST.QUALIFIED_ALIAS_MEMBER:
				List<String> identifierList = new LinkedList<String>();
				// TODO
				return identifierList;
			case CSharp4AST.VARIABLE_DECLARATOR:
				String varName = walk(commonTreeNode.getFirstChildWithType(CSharp4AST.IDENTIFIER), null, String.class);

				DataElement variable_declarator = KDMChildHelper.getChildItemByName(varName, parent, DataElement.class);
				declarations.peek().add(variable_declarator);

				AbstractCodeElement initializer = walk(
						commonTreeNode.getFirstChildWithType(CSharp4AST.VARIABLE_INITIALIZER), variable_declarator,
						AbstractCodeElement.class);
				if (initializer != null) { // is optional
					HasValue varHasValue = KDMElementFactory.createHasValue(variable_declarator, initializer);
					variable_declarator.getCodeRelation().add(varHasValue);
					if (initializer.eContainer() == null) {
						MoDiscoKDM.add(externalCodeModel, initializer);
					}
				}

				return variable_declarator;
			case CSharp4AST.CONSTANT_DECLARATOR:
				// FIXME
				// return
				// declaratorTransformer.transformConstant(commonTreeNode,
				// parent,
				// externalCodeModel, declarations);
				break;
			case CSharp4AST.TYPE:
				try {
					List<CodeItem> _declarationsForTypeParams = identifierResolver.resolveDeclarations(declarations);
					return typeTransformator.transform(commonTreeNode, parent, _declarationsForTypeParams,
							internalNamespaceStack, compilationUnit);
				} catch (RuntimeException e) {
					throw new TypeNotFoundException(e.getLocalizedMessage(), e);
				}
				// break;
			case CSharp4AST.IDENTIFIER:
				return node.getText();
			case CSharp4AST.ATTRIBUTE:
				// TODO
				// ATTRIBUTE_NAME;
				// POSITIONAL_ARGUMENT_LIST:
				break;
			case CSharp4AST.STRING_LITERAL:
				return literalTransformer.transformString(commonTreeNode, parent);
			case CSharp4AST.INTEGER_LITERAL:
				return literalTransformer.transformInteger(commonTreeNode, parent);
			case CSharp4AST.REAL_LITERAL:
				return literalTransformer.transformReal(commonTreeNode, parent);
			case CSharp4AST.CHARACTER_LITERAL:
				return literalTransformer.transformCharacter(commonTreeNode, parent);
			case CSharp4AST.NULL:
				return literalTransformer.transformNull(commonTreeNode, parent);
			case CSharp4AST.TRUE:
			case CSharp4AST.FALSE:
				return literalTransformer.transformBoolean(commonTreeNode, parent);
			case CSharp4AST.IF:
				ActionElement ifStatement = MoDiscoKDM.createIfStatement();

				AbstractCodeElement conditionRes = walk(commonTreeNode.getFirstChildWithType(CSharp4AST.CONDITION),
						parent, AbstractCodeElement.class);
				if (conditionRes == null) {
					conditionRes = KDMElementFactory.createActionElementFallback();
				}
				ActionElement condition;
				if (conditionRes instanceof DataElement) {
					condition = KDMElementFactory.createReadAccess(ifStatement, (DataElement) conditionRes);
				} else {
					condition = (ActionElement) conditionRes;
				}
				condition.setKind("if condition");

				ActionElement thenBranch = walk(commonTreeNode.getFirstChildWithType(CSharp4AST.THEN), parent,
						ActionElement.class);
				if (thenBranch == null) {
					thenBranch = KDMElementFactory.createActionElementFallback();
				}
				thenBranch.setKind("then branch");

				ActionElement elseBranch = walk(commonTreeNode.getFirstChildWithType(CSharp4AST.ELSE), parent,
						ActionElement.class);
				if (elseBranch != null) elseBranch.setKind("else branch");

				ifStatement.getCodeElement().add(condition);
				ifStatement.getCodeElement().add(thenBranch);
				if (elseBranch != null) ifStatement.getCodeElement().add(elseBranch);
				return ifStatement;
			case CSharp4AST.BOOL_NOT:
				return operatorTransformer.transformPrefixUnaryOp(node, parent, "NOT");
			case CSharp4AST.CONDITIONAL_EXPRESSION:
				ActionElement condExpr = MoDiscoKDM.createConditionalExpression();
				for (int i = 0; i < node.getChildCount(); i++) {
					AbstractCodeElement expr = walk(node.getChild(i), parent, AbstractCodeElement.class);
					if (expr == null) {
						expr = KDMElementFactory.createActionElementFallback();
					} else if (expr instanceof DataElement) {
						expr = KDMElementFactory.createReadAccess(condExpr, (DataElement) expr);
					}
					condExpr.getCodeElement().add(expr);
				}
				return condExpr;
			case CSharp4AST.WHILE:
				return loopTransformator.transformWhile(commonTreeNode, parent, declarations);
			case CSharp4AST.DO:
				return loopTransformator.transformDoWhile(commonTreeNode, parent, declarations);
			case CSharp4AST.FOR:
				return loopTransformator.transformFor(commonTreeNode, parent, declarations);
			case CSharp4AST.FOREACH:
				return loopTransformator.transformForeach(commonTreeNode, parent, declarations);
			case CSharp4AST.OP_COALESCING:
				break;
			case CSharp4AST.OP_OR:
				return operatorTransformer.transformBinaryOp(node, parent, "OP_OR");
			case CSharp4AST.OP_AND:
				return operatorTransformer.transformBinaryOp(node, parent, "OP_AND");
			case CSharp4AST.BITWISE_OR:
				return operatorTransformer.transformBinaryOp(node, parent, "BITWISE_OR");
			case CSharp4AST.CARET:
				break;
			case CSharp4AST.AMP:
				break;
			case CSharp4AST.OP_EQ:
				return operatorTransformer.transformBinaryOp(node, parent, "EQUALS");
			case CSharp4AST.OP_NE:
				return operatorTransformer.transformBinaryOp(node, parent, "NOT_EQUAL");
			case CSharp4AST.LT:
				return operatorTransformer.transformBinaryOp(node, parent, "LESS");
			case CSharp4AST.GT:
				return operatorTransformer.transformBinaryOp(node, parent, "GREATER");
			case CSharp4AST.OP_LE:
				return operatorTransformer.transformBinaryOp(node, parent, "LESS_EQUAL");
			case CSharp4AST.OP_GE:
				return operatorTransformer.transformBinaryOp(node, parent, "GREATER_EQUAL");
			case CSharp4AST.IS:
				try {
					return operatorTransformer.transformBinaryOp(node, parent, "IS");
				} catch (RuntimeException e) {
					throw new ExpressionNotSupported(e.getLocalizedMessage());
				}
			case CSharp4AST.AS:
				try {
					return operatorTransformer.transformBinaryOp(node, parent, "AS");
				} catch (RuntimeException e) {
					throw new ExpressionNotSupported(e.getLocalizedMessage());
				}
			case CSharp4AST.OP_LEFT_SHIFT:
				return operatorTransformer.transformBinaryOp(node, parent, "OP_LEFT_SHIFT");
			case CSharp4AST.OP_RIGHT_SHIFT:
				return operatorTransformer.transformBinaryOp(node, parent, "OP_RIGHT_SHIFT");
			case CSharp4AST.PLUS:
				return operatorTransformer.transformBinaryOp(node, parent, "PLUS");
			case CSharp4AST.MINUS:
				return operatorTransformer.transformBinaryOp(node, parent, "MINUS");
			case CSharp4AST.STAR:
				break;
			case CSharp4AST.DIV:
				return operatorTransformer.transformBinaryOp(node, parent, "DIV");
			case CSharp4AST.PERCENT:
				return operatorTransformer.transformBinaryOp(node, parent, "MODULO");
			case CSharp4AST.RETURN:
				ActionElement returnStmt = MoDiscoKDM.createReturnStatement();
				AbstractCodeElement returnExpr = walk(commonTreeNode.getChild(0), parent, AbstractCodeElement.class);
				if (returnExpr == null) {
					returnExpr = KDMElementFactory.createActionElementFallback();
				} else if (returnExpr instanceof DataElement) {
					returnExpr = KDMElementFactory.createReadAccess(returnStmt, (DataElement) returnExpr);
				}
				if (!(returnExpr instanceof ActionElement)) {
					throw new ExpressionNotSupported("returnExpr is not of type ActionElement - "
							+ returnExpr.getClass());
				}
				returnStmt.getCodeElement().add(returnExpr);
				return returnStmt;
			case CSharp4AST.THROW:
				ActionElement throwStmt = MoDiscoKDM.createThrowStatement();
				AbstractCodeElement throwExpr = walk(commonTreeNode.getChild(0), parent, AbstractCodeElement.class);
				if (throwExpr == null) {
					throwExpr = KDMElementFactory.createActionElementFallback();
				} else if (throwExpr instanceof DataElement) {
					returnExpr = KDMElementFactory.createReadAccess(throwStmt, (DataElement) throwExpr);
				}
				if (!(throwExpr instanceof ActionElement)) {
					throw new ExpressionNotSupported("throwExpr is not of type ActionElement - " + throwExpr.getClass());
				}
				throwStmt.getCodeElement().add(throwExpr);
				return throwStmt;
				// case CSharp4AST.OBJECT_CREATION_EXPRESSION:
				// ActionElement instanceCreation =
				// MoDiscoKDM.createClassInstanceCreationExpression();
				//
				// Datatype objectType =
				// walk(commonTreeNode.getFirstChildWithType(CSharp4AST.TYPE),
				// parent, Datatype.class);
				//
				// // TODO actual parameters
				//
				// // ControlElement constructorDecl = null; //TODO
				// // Calls instCreationCall =
				// KDMElementFactory.createCalls(instanceCreation,
				// constructorDecl);
				// //
				// instanceCreation.getActionRelation().add(instCreationCall);
				//
				// Creates typeCreation =
				// KDMElementFactory.createCreates(instanceCreation,
				// objectType);
				// instanceCreation.getActionRelation().add(typeCreation);
				//
				// return instanceCreation;
			case CSharp4AST.EXPRESSION_STATEMENT:
				ActionElement expressionStatement = MoDiscoKDM.createExpressionStatement();

				try {
					AbstractCodeElement exprStmt = walk(node.getChild(0), parent, AbstractCodeElement.class);
					if (exprStmt instanceof DataElement) {
						exprStmt = KDMElementFactory.createReadAccess(expressionStatement, (DataElement) exprStmt);
					}
					expressionStatement.getCodeElement().add(exprStmt);
				} catch (IllegalStateException e) {
					throw new ExpressionNotSupported(e.getLocalizedMessage());
				}

				return expressionStatement;
			case CSharp4AST.ASSIGNMENT_OPERATOR:
				ActionElement assignment = MoDiscoKDM.createAssignment(node.getChild(0).getText());

				AbstractCodeElement asLeft = walk(commonTreeNode.getChild(1), parent, AbstractCodeElement.class);
				if (asLeft == null) { // TODO if everything is supported, remove
					// this if
					asLeft = KDMElementFactory.createActionElementFallback();
				} else if (asLeft instanceof DataElement) {
					asLeft = KDMElementFactory.createWriteAccess(assignment, (DataElement) asLeft);
				}

				AbstractCodeElement asRight = walk(commonTreeNode.getChild(2), parent, AbstractCodeElement.class);
				if (asRight == null) { // TODO if everything is supported,
					// remove
					// this if
					asRight = KDMElementFactory.createActionElementFallback();
				} else if (asRight instanceof DataElement) {
					asRight = KDMElementFactory.createReadAccess(assignment, (DataElement) asRight);
				}

				assignment.getCodeElement().add(asLeft);
				assignment.getCodeElement().add(asRight);
				return assignment;
			case CSharp4AST.LOCAL_VARIABLE_DECLARATOR:
				return declaratorTransformer.transformLocalVariable(commonTreeNode, parent, declarations);
			case CSharp4AST.SIMPLE_NAME:
				String identifier = walk(commonTreeNode.getFirstChildWithType(CSharp4AST.IDENTIFIER), parent,
						String.class);
				return identifier;
			case CSharp4AST.MEMBER_ACCESS:
				List<String> accessList = new LinkedList<String>();

				Object accessPrefix = walk(commonTreeNode.getChild(0), parent);
				if (accessPrefix == null) {
					throw new ExpressionNotSupported(String.valueOf(commonTreeNode.getChild(0)));
				} else if (accessPrefix instanceof String) {
					accessList.add((String) accessPrefix);
				} else if (accessPrefix instanceof List) {
					accessList.addAll((Collection<? extends String>) accessPrefix);
				} else {
					LOGGER.unsupported("Cannot process member access of type => " + accessPrefix.getClass());
					throw new ExpressionNotSupported(String.valueOf(accessPrefix));
				}

				String memberIdent = walk(commonTreeNode.getFirstChildWithType(CSharp4AST.IDENTIFIER), parent,
						String.class);
				accessList.add(memberIdent);

				// invocationPath.add(memberIdent);
				return accessList;
			case CSharp4AST.UNARY_EXPRESSION:
				// resolve expression
				// for (int i = 0; i < node.getChildCount(); i++) {

				// TODO limited support: only one child
				Tree child = node.getChild(0);
				Object unaryExpr = walk(child, parent);
				if (unaryExpr == null) throw new ExpressionNotSupported("unaryExpr == null");

				try {
					if (child.getType() == CSharp4AST.SIMPLE_NAME || child.getType() == CSharp4AST.MEMBER_ACCESS) {

						if (unaryExpr instanceof String) {
							memberIdent = (String) unaryExpr;
							typeName = Arrays.asList(memberIdent);
						} else
							typeName = (List<String>) unaryExpr;

						List<CodeItem> _declarations = identifierResolver.resolveDeclarations(declarations);

						unaryExpr = identifierResolver.resolveType(typeName, _declarations, internalNamespaceStack,
								compilationUnit, parent);
						// TODO return ActionElement
						// unaryExpr = ;
					} // else: result is already resolved
					return unaryExpr;
				} catch (java.lang.IllegalStateException e) {
					// TODO remove if everything is supported
					throw new ExpressionNotSupported(e.getLocalizedMessage());
				} catch (TypeNotFoundException e) {
					// TODO remove if everything is supported
					throw new ExpressionNotSupported(e.getLocalizedMessage());
				}
			case CSharp4AST.METHOD_INVOCATION:
				try {
					ActionElement methodInvocation = MoDiscoKDM.createMethodInvocation();

					// builds invocationPath
					result = walk(node.getChild(0), parent);
					List<String> invocationPath;
					if (result == null) {
						throw new ExpressionNotSupported("result == null");
					} else if (result instanceof String) {
						invocationPath = new LinkedList<String>();
						invocationPath.add((String) result);
						throw new ExpressionNotSupported("Method invocation without type prefix is not yet supported.");
					} else if (result instanceof List) {
						invocationPath = (List<String>) result;
					} else {
						throw new ExpressionNotSupported("Unexpected method invocation path: " + result.getClass());
					}

					// System.out.println("invocationPath: " + invocationPath);

					List<String> methodOwnerPath = invocationPath.subList(0, invocationPath.size() - 1);
					List<CodeItem> _declarations = identifierResolver.resolveDeclarations(declarations);

					// get method owner (e.g. class)
					CodeItem methodOwner = identifierResolver.resolveType(methodOwnerPath, _declarations,
							internalNamespaceStack, compilationUnit, parent);
					// System.out.println("methodOwner: " + methodOwner);

					StringBuilder methodIdentBuilder = new StringBuilder();
					methodIdentBuilder.append(invocationPath.get(invocationPath.size() - 1));
					methodIdentBuilder.append("(");
					for (int i = 1; i < node.getChildCount(); i++) {
						Tree argNode = node.getChild(i);
						if (argNode.getType() == CSharp4AST.ARGUMENT) {
							AbstractCodeElement arg = walk(argNode, parent, AbstractCodeElement.class);
							if (arg == null) {
								throw new ExpressionNotSupported("Method invocation argument " + argNode.getText()
										+ " is not yet supported");
							}

							if (arg instanceof DataElement) {
								// ValueElement, ParameterUnit
								Datatype argType = ((DataElement) arg).getType();
								if (argType == null) {
									throw new ExpressionNotSupported(
											"[METHOD_INVOCATION] parent is an LOGGER.unsupported type parameter");
								}
								methodIdentBuilder.append(argType.getName());

								arg = KDMElementFactory.createReadAccess(methodInvocation, (DataElement) arg);
							} else {
								throw new ExpressionNotSupported(
										"[METHOD_INVOCATION] LOGGER.unsupported method invocation argument: " + arg);
							}

							methodInvocation.getCodeElement().add(arg);

							if (i < node.getChildCount() - 1) {
								methodIdentBuilder.append(",");
							}
						}
					}
					methodIdentBuilder.append(")");

					CodeItem method;
					method = identifierResolver.resolveFurtherIdentifier(methodOwner, methodIdentBuilder.toString());
					if (method instanceof DataElement) {
						DataElement array = (DataElement) method;
						// TODO workaround to support methods of C# type "Array"
						if (array.getType() instanceof ArrayType) {
							// System.out.println("new method for array");
							method = KDMElementFactory.createMethodUnit();
							method.setName(methodIdentBuilder.toString());
							methodInvocation.getCodeElement().add(method);
						}
					}

					// System.out.println("Invoked method: " + method);

					Calls calls = KDMElementFactory.createCalls(methodInvocation, (ControlElement) method);
					methodInvocation.getActionRelation().add(calls);

					return methodInvocation;
				} catch (RuntimeException e) {
					// for those cases that are not yet supported
					throw new ExpressionNotSupported(e.getLocalizedMessage());
				}
			case CSharp4AST.ARGUMENT:
				String argName = walk(commonTreeNode.getFirstChildWithType(CSharp4AST.IDENTIFIER), parent, String.class);
				if (argName != null) {
					// TODO argument name
				}

				AbstractCodeElement arg = walk(commonTreeNode.getFirstChildWithType(CSharp4AST.ARGUMENT_VALUE), parent,
						AbstractCodeElement.class);

				return arg;
			case CSharp4AST.ARGUMENT_VALUE:
				Tree exprNode = node;

				int childIndex = 0;
				Object outNode = walk(commonTreeNode.getFirstChildWithType(CSharp4AST.OUT), parent);
				if (outNode != null) {
					childIndex = 1;
				}

				Object refNode = walk(commonTreeNode.getFirstChildWithType(CSharp4AST.REF), parent);
				if (refNode != null) {
					childIndex = 1;
				}

				AbstractCodeElement argExpr = walk(exprNode.getChild(childIndex), parent, AbstractCodeElement.class);
				// TODO set way of parameter passing (out, ref)

				return argExpr;
			case CSharp4AST.POST_INC:
				return operatorTransformer.transformUnaryOp(commonTreeNode, parent, "INCREMENT", declarations,
						internalNamespaceStack, compilationUnit);
			case CSharp4AST.POST_DEC:
				return operatorTransformer.transformUnaryOp(commonTreeNode, parent, "DECREMENT", declarations,
						internalNamespaceStack, compilationUnit);
			case CSharp4AST.BASE:
			case CSharp4AST.THIS:
				throw new ExpressionNotSupported(node.getText());
			default:
				LOGGER.skip(commonTreeNode.getText());
				break;
		}

		return null;
	}

	private String getTypeName(final Tree typeTree) {
		StringBuilder builder = new StringBuilder();

		Tree baseTypeNode = typeTree.getChild(0);
		switch (baseTypeNode.getType()) {
			case CSharp4AST.NAMESPACE_OR_TYPE_NAME:
				// composite type (e.g. class or interface)
				@SuppressWarnings("unchecked")
				List<String> qualifiedBaseType = walk(baseTypeNode, null, List.class);

				// ignore qualified parameter types
				builder.append(qualifiedBaseType.get(qualifiedBaseType.size() - 1));
				// builder.append(ListUtil.combine(qualifiedBaseType, "."));
				break;
			default: // OBJECT, STRING, VOID, IDENTIFIER(dynamic), and primitive
				// types
				builder.append(baseTypeNode.getText());
				break;
		}

		for (int i = 1; i < typeTree.getChildCount(); i++) {
			Tree typeExtension = typeTree.getChild(i);
			// INTERR, rank_specifier, STAR
			switch (typeExtension.getType()) {
				case CSharp4AST.INTERR:
					LOGGER.unsupported("INTERR is not yet supported");
					break;
				case CSharp4AST.RANK_SPECIFIER:
					int numCommas = typeExtension.getChildCount();
					do {
						builder.append("[]");
					} while (numCommas-- > 0);
					break;
				case CSharp4AST.STAR:
					builder.append("*");
					break;
				default:
					break;
			}
		}

		return builder.toString();
	}

	@Override
	@SuppressWarnings("unchecked")
	public <T> T walk(final Tree node, final CodeItem parent, final Class<T> class1) {
		Object result = walk(node, parent);
		return ((T) result);
	}

	public void setType(final DataElement dataElement, final String predefinedTypeName) {
		try {
			Datatype type = genericLanguageUnitCache.getDatatypeFromString(predefinedTypeName);
			dataElement.setType(type);
		} catch (NotInCacheException e) {
			// TODO search for the type in internal and external namespaces
			// throw new IllegalStateException("'" + predefinedTypeName +
			// "' ---->" +
			// e.getMessage(), e);
			LOGGER.warning("Could not find type in cache: '" + predefinedTypeName + "'");
		}
	}

	@Override
	public void transform(final CommonTree node) {
		transform(node, SourceFactory.eINSTANCE.createSourceFile());
	}
}
