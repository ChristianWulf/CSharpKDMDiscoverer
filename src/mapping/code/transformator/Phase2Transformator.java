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
import java.util.Collections;
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
import mapping.code.namespace.NamespaceBuilder;
import mapping.code.namespace.NamespaceSearcher;
import mapping.code.namespace.NamespaceStack;
import mapping.code.resolver.IdentifierResolver;
import mapping.util.KDMChildHelper;
import mapping.util.TreeHelper;

import org.antlr.runtime.tree.CommonTree;
import org.antlr.runtime.tree.Tree;
import org.eclipse.gmt.modisco.omg.kdm.code.AbstractCodeElement;
import org.eclipse.gmt.modisco.omg.kdm.code.AbstractCodeRelationship;
import org.eclipse.gmt.modisco.omg.kdm.code.ClassUnit;
import org.eclipse.gmt.modisco.omg.kdm.code.CodeFactory;
import org.eclipse.gmt.modisco.omg.kdm.code.CodeItem;
import org.eclipse.gmt.modisco.omg.kdm.code.CodeModel;
import org.eclipse.gmt.modisco.omg.kdm.code.CompilationUnit;
import org.eclipse.gmt.modisco.omg.kdm.code.DataElement;
import org.eclipse.gmt.modisco.omg.kdm.code.Datatype;
import org.eclipse.gmt.modisco.omg.kdm.code.EnumeratedType;
import org.eclipse.gmt.modisco.omg.kdm.code.Extends;
import org.eclipse.gmt.modisco.omg.kdm.code.Implements;
import org.eclipse.gmt.modisco.omg.kdm.code.Imports;
import org.eclipse.gmt.modisco.omg.kdm.code.InterfaceUnit;
import org.eclipse.gmt.modisco.omg.kdm.code.Module;
import org.eclipse.gmt.modisco.omg.kdm.code.Namespace;
import org.eclipse.gmt.modisco.omg.kdm.code.ParameterUnit;
import org.eclipse.gmt.modisco.omg.kdm.code.TemplateParameter;
import org.eclipse.gmt.modisco.omg.kdm.code.Value;
import org.eclipse.gmt.modisco.omg.kdm.source.SourceFile;

import util.ListUtil;
import util.Logger;
import de.cau.chw.csharp.resolution.Mapper;
import de.cau.chw.csharp.resolution.MapperFiller;

public class Phase2Transformator extends AbstractTransformator implements IKDMMapper<CompilationUnit> {

	private static final Logger				LOG							= new Logger(
			Phase2Transformator.class);

	private static final String				DYNAMIC						= "dynamic";
	private final GenericLanguageUnitCache	genericLanguageUnitCache;
	private NamespaceStack					internalNamespaceStack;
	private final NamespaceSearcher			searcher;
	private final IdentifierResolver		identifierResolver;
	private final Namespace					internalGlobalNamespace;
	private final Namespace					externalGlobalNamespace;
	private final NamespaceBuilder			builder;

	private final TypeTransformator			typeTransformator;

	private final MethodTransformator		methodTransformator;

	private final DeclaratorTransformer		declaratorTransformer;

	private final Mapper					mapper						= new Mapper();

	private final Deque<List<CodeItem>>		declarations				= new LinkedList<List<CodeItem>>();
	private final Deque<List<CodeItem>>		typeParameterDefinitions	= new LinkedList<List<CodeItem>>();

	private final ParameterTransformer		parameterTransformer;
	private final ModifiersTransformator	modifiersTransformator;

	private CompilationUnit					compilationUnit;

	private final CodeModel					internalCodeModel;

	private final Map<String, Imports>		aliases						= new HashMap<String, Imports>();

	public Phase2Transformator(final GenericLanguageUnitCache genericLanguageUnitCache,
			final CodeModel internalCodeModel, final Module valueRepository, final CodeModel externalCodeModel,
			final ExternalDatatypeInfoRepository externalDatatypeInfoRepository) {
		this.genericLanguageUnitCache = genericLanguageUnitCache;
		this.internalCodeModel = internalCodeModel;
		// this.externalCodeModel = externalCodeModel;

		this.internalGlobalNamespace = NamespaceSearcher.getGlobalNamespaceFrom(internalCodeModel);
		this.externalGlobalNamespace = NamespaceSearcher.getGlobalNamespaceFrom(externalCodeModel);

		this.searcher = new NamespaceSearcher(internalGlobalNamespace, externalGlobalNamespace);
		this.identifierResolver = new IdentifierResolver(searcher, externalDatatypeInfoRepository, aliases,
				externalCodeModel);
		this.builder = new NamespaceBuilder(searcher, identifierResolver);

		this.typeTransformator = new TypeTransformator(identifierResolver, genericLanguageUnitCache, this);
		this.modifiersTransformator = new ModifiersTransformator();
		this.methodTransformator = new MethodTransformator(this, modifiersTransformator);
		this.declaratorTransformer = new DeclaratorTransformer(this, modifiersTransformator, externalCodeModel);
		this.parameterTransformer = new ParameterTransformer(this);

		// TODO mapperFiller as constructor parameter
		new MapperFiller().fillUsingDirectives(mapper);
	}

	@Override
	public void transform(final CommonTree node) {
		throw new IllegalStateException();
	}

	@Override
	public void transform(final CommonTree node, final SourceFile sourceFile) {
		LOG.info("Start transforming...");
		this.internalNamespaceStack = new NamespaceStack(internalGlobalNamespace, searcher, identifierResolver);
		this.compilationUnit = KDMChildHelper.getCompilationUnit(sourceFile, internalCodeModel);

		// System.out.println("Processing..." + sourceFile.getName());
		try {
			for (int i = 0; i < node.getChildCount(); i++) {
				walk(node.getChild(i), internalGlobalNamespace);
			}
		} catch (RuntimeException e) {
			// additionally display SourceFile upon exception
			LOG.error("Exception while processing '" + sourceFile.getPath() + "'", e);
			throw e;
		}

		LOG.info("done.");
	}

	@Override
	@SuppressWarnings("unchecked")
	protected Object walk(final Tree node, final CodeItem parent) throws TypeNotFoundException {
		// an optional node that is not present in the current context, is null
		if (node == null) {
			return null;
		}

		CommonTree commonTreeNode = (CommonTree) node;
		LOG.info("Type: " + commonTreeNode.getType());
		LOG.info("Text: " + commonTreeNode.getText());
		LOG.info("Children: " + commonTreeNode.getChildren());

		Object result;
		CommonTree modifiers;
		List<AbstractCodeRelationship> classRelations;

		switch (commonTreeNode.getType()) {
			case CSharp4AST.NAMESPACE:
				CommonTree qid = (CommonTree) commonTreeNode.getFirstChildWithType(CSharp4AST.QUALIFIED_IDENTIFIER);
				List<String> identifiers = TreeHelper.treeListToStringList(qid.getChildren());

				Namespace namespace = internalNamespaceStack.updateFromCurrentNamespace(identifiers);

				walk(commonTreeNode.getFirstChildWithType(CSharp4AST.EXTERN_ALIAS_DIRECTIVES), namespace);
				walk(commonTreeNode.getFirstChildWithType(CSharp4AST.USING_DIRECTIVES), namespace);
				walk(commonTreeNode.getFirstChildWithType(CSharp4AST.NAMESPACE_MEMBER_DECLARATIONS), namespace);

				internalNamespaceStack.popCurrentNamespace(identifiers.size());
				break;
			case CSharp4AST.QUALIFIED_IDENTIFIER:
			case CSharp4AST.EXTERN_ALIAS_DIRECTIVES:
			case CSharp4AST.USING_DIRECTIVES:
			case CSharp4AST.ATTRIBUTES:
			case CSharp4AST.NAMESPACE_MEMBER_DECLARATIONS:
			case CSharp4AST.CLASS_MEMBER_DECLARATIONS:
			case CSharp4AST.INTERFACE_MEMBER_DECLARATIONS:
			case CSharp4AST.CONSTANT_DECLARATORS:
			case CSharp4AST.STRUCT_MEMBER_DECLARATIONS:
				// case CSharp4AST.VARIANT_TYPE_PARAMETERS:
				for (int i = 0; i < commonTreeNode.getChildCount(); i++) {
					walk(commonTreeNode.getChild(i), parent);
				}
				break;
			case CSharp4AST.EXTERN:
				// TODO
				break;
			case CSharp4AST.USING_ALIAS_DIRECTIVE:
				String alias = walk(commonTreeNode.getFirstChildWithType(CSharp4AST.IDENTIFIER), parent, String.class);

				List<String> usingAliasNamespace = walk(
						commonTreeNode.getFirstChildWithType(CSharp4AST.NAMESPACE_OR_TYPE_NAME), parent, List.class);

				// FIXME a using directive can not only be a namespace, but also a
				// type
				Namespace foreignAliasNamespace = searcher.findNamespaceFromGlobals(usingAliasNamespace,
						NamespaceSearcher.nsNullListener);
				// if not found, create hierarchy below external global namespace
				if (foreignAliasNamespace == null) {
					try {
						foreignAliasNamespace = builder.updateNamespaceHierarchy(usingAliasNamespace,
								externalGlobalNamespace);
					} catch (ClassCastException e) {
						// TODO: handle exception if using is a type, not a namespace
					}
					// for the case of a dead using or type
					if (foreignAliasNamespace == null) {
						LOG.unsupported("using alias directive only supports namespaces, not types so far.");
						break;
						// foreignAliasNamespace =
						// builder.updateNamespaceHierarchy(usingAliasNamespace,
						// externalGlobalNamespace);
					}
				}

				Imports aliasImports = KDMElementFactory.createImports(parent, foreignAliasNamespace);
				/*
				 * A using directive only applies to the owning CompilationUnit,
				 * thus we add this relationship to the CompilationUnit, not to any
				 * Namespace
				 */
				compilationUnit.getCodeRelation().add(aliasImports);

				aliases.put(alias, aliasImports);
				break;
			case CSharp4AST.USING_NAMESPACE_DIRECTIVE:
				List<String> usingNamespace = walk(
						commonTreeNode.getFirstChildWithType(CSharp4AST.NAMESPACE_OR_TYPE_NAME), parent, List.class);

				// usingType: a namespace or a type (e.g. class)
				Object usingType = mapper.getUsingDirective(ListUtil.combine(usingNamespace, "."));

				// @Deprecated code block
				Namespace foreignNamespace;
				{
					foreignNamespace = searcher.findNamespaceFromGlobals(usingNamespace,
							NamespaceSearcher.nsNullListener);

					// System.out.println(usingNamespace+": "+foreignNamespace);

					// if not found, create hierarchy below external global namespace
					if (foreignNamespace == null) {
						LOG.fine("Namespace '" + usingNamespace
								+ "' is not yet added to the internal or external namespace hierarchy.");
						try {
							foreignNamespace = builder
									.updateNamespaceHierarchy(usingNamespace, externalGlobalNamespace);
						} catch (ClassCastException cce) {
							LOG.unsupported("Using directive does not support classes (e.g. System.Buffer) so far.");
							return null;
						}

						// System.out.println("\t"+foreignNamespace);

						if (foreignNamespace == null) { // for the case of a dead using
							// directive
							LOG.warning("Using directive refers neither to an internal namespace "
									+ "nor to an external namespace that exists on the file system.\n"
									+ "Maybe it is a dead using directive, i.e., it is not existent anymore.\n"
									+ "\tUSING_NAMESPACE_DIRECTIVE = " + usingNamespace);
							// throw new
							// IllegalStateException("USING_NAMESPACE_DIRECTIVE = "
							// + usingNamespace);
							break; // do not create import relationship
						}
					} else {
						LOG.fine("found namespace: " + foreignNamespace);
					}
				}

				Imports imports = KDMElementFactory.createImports(parent, foreignNamespace);

				/*
				 * A using directive only applies to the owning CompilationUnit,
				 * thus we add this relationship to the CompilationUnit, not to any
				 * namespaces
				 */
				compilationUnit.getCodeRelation().add(imports);
				break;
			case CSharp4AST.CLASS:
				typeParameterDefinitions.push(newNotNullLinkedList());
				declarations.push(newNotNullLinkedList());
				String className = walk(commonTreeNode.getFirstChildWithType(CSharp4AST.IDENTIFIER), null, String.class);
				ClassUnit classUnit = KDMChildHelper.getAbstractCodeElementByName(compilationUnit.getCodeElement(),
						className, ClassUnit.class);

				// walk(commonTreeNode.getFirstChildWithType(CSharp4AST.TYPE_PARAMETERS),
				// classUnit);

				try {
					classRelations = walk(commonTreeNode.getFirstChildWithType(CSharp4AST.EXTENDS_OR_IMPLEMENTS),
							classUnit, List.class);
					if (classRelations != null) {
						// TODO perhaps use parent in EXTENDS_OR_IMPLEMENTS instead
						// of
						// the following
						for (AbstractCodeRelationship relation : classRelations) {
							if (relation instanceof Extends) {
								((Extends) relation).setFrom(classUnit);
							} else if (relation instanceof Implements) {
								((Implements) relation).setFrom(classUnit);
							}
						}
						classUnit.getCodeRelation().addAll(classRelations);
					}
				} catch (TypeNotFoundException e) {
					// skip
					System.err.println(e);
				}

				walk(commonTreeNode.getFirstChildWithType(CSharp4AST.CLASS_MEMBER_DECLARATIONS), classUnit);

				// walk(commonTreeNode.getFirstChildWithType(CSharp4AST.ATTRIBUTES),
				// classUnit);

				// modifiers
				modifiers = (CommonTree) commonTreeNode.getFirstChildWithType(CSharp4AST.MODIFIERS);
				modifiersTransformator.transformModifiers(classUnit, modifiers, "private");

				declarations.pop();
				typeParameterDefinitions.pop();
				return classUnit;
			case CSharp4AST.STRUCT:
				declarations.push(newNotNullLinkedList());
				String structName = commonTreeNode.getFirstChildWithType(CSharp4AST.IDENTIFIER).getText();
				ClassUnit structType = KDMChildHelper.getAbstractCodeElementByName(compilationUnit.getCodeElement(),
						structName, ClassUnit.class);

				// TYPE_PARAMETERS
				// IMPLEMENTS
				// TYPE_PARAMETER_CONSTRAINTS_CLAUSES

				walk(commonTreeNode.getFirstChildWithType(CSharp4AST.STRUCT_MEMBER_DECLARATIONS), structType);

				modifiers = (CommonTree) commonTreeNode.getFirstChildWithType(CSharp4AST.MODIFIERS);
				modifiersTransformator.transformModifiers(structType, modifiers, "private");

				LOG.warning("LOGGER.unsupported: struct is not yet supported");
				declarations.pop();
				return structType;
			case CSharp4AST.INTERFACE:
				typeParameterDefinitions.push(newNotNullLinkedList());
				declarations.push(newNotNullLinkedList());
				String interfaceName = commonTreeNode.getFirstChildWithType(CSharp4AST.IDENTIFIER).getText();
				InterfaceUnit interfaceUnit = KDMChildHelper.getAbstractCodeElementByName(
						compilationUnit.getCodeElement(), interfaceName, InterfaceUnit.class);

				// walk(commonTreeNode.getFirstChildWithType(CSharp4AST.VARIANT_TYPE_PARAMETERS),
				// interfaceUnit);
				List<AbstractCodeRelationship> interfaceRelations = walk(
						commonTreeNode.getFirstChildWithType(CSharp4AST.IMPLEMENTS), interfaceUnit, List.class);
				if (interfaceRelations != null) {
					// TODO perhaps use parent in IMPLEMENTS instead of the
					// following
					for (AbstractCodeRelationship relation : interfaceRelations) {
						((Extends) relation).setFrom(interfaceUnit);
					}
					interfaceUnit.getCodeRelation().addAll(interfaceRelations);
				}

				// modifiers
				MoDiscoKDM.createAndAddAccessModifierToElement("public", interfaceUnit);

				// walk(commonTreeNode
				// .getFirstChildWithType(CSharp4AST.TYPE_PARAMETER_CONSTRAINTS_CLAUSES),
				// interfaceUnit);
				walk(commonTreeNode.getFirstChildWithType(CSharp4AST.INTERFACE_MEMBER_DECLARATIONS), interfaceUnit);

				declarations.pop();
				typeParameterDefinitions.pop();
				break;
			case CSharp4AST.ENUM:
				declarations.push(newNotNullLinkedList());
				String enumName = commonTreeNode.getFirstChildWithType(CSharp4AST.IDENTIFIER).getText();
				EnumeratedType enumeratedType = KDMChildHelper.getAbstractCodeElementByName(
						compilationUnit.getCodeElement(), enumName, EnumeratedType.class);

				Tree enumBaseType = commonTreeNode.getFirstChildWithType(CSharp4AST.ENUM_EXTENDS);
				if (enumBaseType != null) {
					enumBaseType.getText();
					// TODO
					LOG.unsupported("Super types for ENUMs are not yet supported.");
				}

				// modifiers
				modifiers = (CommonTree) commonTreeNode.getFirstChildWithType(CSharp4AST.MODIFIERS);
				modifiersTransformator.transformModifiers(enumeratedType, modifiers, "public");

				List<Value> enumDeclarations = walk(
						commonTreeNode.getFirstChildWithType(CSharp4AST.ENUM_MEMBER_DECLARATIONS), enumeratedType,
						List.class);
				if (enumDeclarations != null) { // optional
					enumeratedType.getCodeElement().addAll(enumDeclarations);
				}

				declarations.pop();
				break;
			case CSharp4AST.DELEGATE:
				LOG.unsupported("DELEGATE is not yet supported.");
				break;
			case CSharp4AST.EXTENDS_OR_IMPLEMENTS:
				classRelations = new LinkedList<AbstractCodeRelationship>();
				/*
				 * here, we have to find the (possibly external) types and detect
				 * whether it is an interface, class, or enum
				 */
				// if (commonTreeNode.getChildCount() == 0) return XXX;

				String firstTypeName = commonTreeNode.getChild(0).getText();
				int firstType = commonTreeNode.getChild(0).getType();

				int childIndex = 0;
				if (firstType == CSharp4AST.OBJECT || firstType == CSharp4AST.STRING || firstTypeName.equals(DYNAMIC)) {
					// definitely extends a class
					try {
						Datatype type = genericLanguageUnitCache.getDatatypeFromString(firstTypeName);
						Extends extendz = CodeFactory.eINSTANCE.createExtends();
						// extendz.setFrom(value); // have to be set from the
						// corresponding
						// class/interface/enum (i.e. the parent)
						extendz.setTo(type);

						classRelations.add(extendz);
					} catch (NotInCacheException e) {
						throw new IllegalStateException(e.getLocalizedMessage(), e);
					}
					// process next child
					childIndex = 1;
				}

				for (int i = childIndex; i < commonTreeNode.getChildCount(); i++) {
					Tree child = commonTreeNode.getChild(i);
					List<String> typeName = walk(child, parent, List.class);
					try {
						AbstractCodeRelationship relation = mapToRelation(typeName, parent);
						classRelations.add(relation);
					} catch (TypeNotFoundException e) {
						// TODO skip for now to transform SharpDevelop
						LOG.error(e);
						continue;
					}
				}

				return classRelations;
			case CSharp4AST.IMPLEMENTS:

				interfaceRelations = new LinkedList<AbstractCodeRelationship>();
				for (int i = 0; i < commonTreeNode.getChildCount(); i++) {
					Tree child = commonTreeNode.getChild(i);
					List<String> typeName = walk(child, parent, List.class);

					try {
						AbstractCodeRelationship relation = mapInterfaceToRelation(typeName, parent);
						interfaceRelations.add(relation);
					} catch (TypeNotFoundException e) {
						// TODO skip for now to transform SharpDevelop
						LOG.error(e);
						continue;
					}
				}
				return interfaceRelations;
			case CSharp4AST.ENUM_MEMBER_DECLARATIONS:
				List<Value> enumDeclars = new LinkedList<Value>();
				for (int i = 0; i < node.getChildCount(); i++) {
					try {
						Tree enumChild = node.getChild(i); // ENUM_MEMBER_DECLARATION
						Value enumValue = walk(enumChild, parent, Value.class);
						enumDeclars.add(enumValue);
					} catch (RuntimeException e) {
						// TODO skip for now to transform SharpDevelop
						LOG.error(e);
					}
				}
				return enumDeclars;
			case CSharp4AST.ENUM_MEMBER_DECLARATION:
				Value value = KDMElementFactory.createValue();
				((EnumeratedType) parent).getCodeElement().add(value);
				value.setType((Datatype) parent);

				// List enumAttrs =
				// walk(commonTreeNode.getFirstChildWithType(CSharp4AST.ATTRIBUTES),
				// parent, List.class);
				// if (enumAttrs != null) {
				// // TODO
				// }

				String enumValue = walk(commonTreeNode.getFirstChildWithType(CSharp4AST.IDENTIFIER), parent,
						String.class);
				value.setName(enumValue);

				AbstractCodeElement enumInit = walk(
						commonTreeNode.getFirstChildWithType(CSharp4AST.ENUM_MEMBER_INITIALIZER), parent,
						AbstractCodeElement.class);
				if (enumInit != null) {
					// TODO
				}

				// value.setExt(value);
				// value.setSize(value);
				return value;
			case CSharp4AST.FIELD_DECL:
			case CSharp4AST.EVENT_VARS_DECL:
				// do not process children of type TYPE
				for (int i = 0; i < commonTreeNode.getChildCount(); i++) {
					Tree child = commonTreeNode.getChild(i);
					if (child.getType() == CSharp4AST.VARIABLE_DECLARATOR) {
						walk(child, parent);
					}
				}
				break;
			case CSharp4AST.PROPERTY_DECL:
				return declaratorTransformer.transformProperty(commonTreeNode, parent, declarations);
			case CSharp4AST.EVENT_PROP_DECL:
				return declaratorTransformer.transformEventProperty(commonTreeNode, parent, declarations);
			case CSharp4AST.INDEXER_DECL:
				return declaratorTransformer.transformIndexer(commonTreeNode, parent, declarations);
			case CSharp4AST.CONSTRUCTOR_DECL:
				// TODO constructor
				break;
			case CSharp4AST.TILDE:
				// TODO destructor
				break;
			case CSharp4AST.METHOD_DECL:
				typeParameterDefinitions.push(newNotNullLinkedList());
				declarations.push(newNotNullLinkedList());
				CodeItem method = methodTransformator.transform(commonTreeNode, parent, internalNamespaceStack);
				declarations.pop();
				typeParameterDefinitions.pop();
				return method;
			case CSharp4AST.MEMBER_NAME:
				List<String> typeName = walk(commonTreeNode.getFirstChildWithType(CSharp4AST.NAMESPACE_OR_TYPE_NAME),
						parent, List.class);

				// limited support TODO
				if (typeName.size() > 1) {
					LOG.warning("LOGGER.unsupported: member_name with more than one IDENTIFIER is not yet supported: "
							+ typeName);
				}
				String memberName = typeName.get(typeName.size() - 1);

				return memberName;
			case CSharp4AST.TYPE_PARAMETERS:
				List<TemplateParameter> typeParams = new LinkedList<TemplateParameter>();
				for (int i = 0; i < node.getChildCount(); i++) {
					TemplateParameter tu = walk(node.getChild(i), parent, TemplateParameter.class);
					typeParams.add(tu);
					typeParameterDefinitions.peek().add(tu);
				}
				return typeParams;
			case CSharp4AST.TYPE_PARAM:
				TemplateParameter typeParam = parameterTransformer.transformTypeParameter(commonTreeNode, parent,
						typeParameterDefinitions);
				return typeParam;
			case CSharp4AST.FORMAL_PARAMETER_LIST:
				List<ParameterUnit> parameters = new LinkedList<ParameterUnit>();
				for (int i = 0; i < commonTreeNode.getChildCount(); i++) {
					Tree child = commonTreeNode.getChild(i);
					ParameterUnit parameter = walk(child, parent, ParameterUnit.class);
					parameter.setPos(i);
					parameters.add(parameter);
				}
				return parameters;
			case CSharp4AST.FIXED_PARAMETER:
				return parameterTransformer.transformFixedParameter(commonTreeNode, parent, declarations);
			case CSharp4AST.PARAMETER_ARRAY:
				return parameterTransformer.transformParameterArray(commonTreeNode, parent, declarations);
			case CSharp4AST.NAMESPACE_OR_TYPE_NAME:
				List<String> nsTypeName = new LinkedList<String>();

				result = walk(commonTreeNode.getFirstChildWithType(CSharp4AST.IDENTIFIER), parent);
				if (result != null) nsTypeName.add((String) result);

				// List<Datatype> typeArguments = walk(
				// commonTreeNode.getFirstChildWithType(CSharp4AST.TYPE_ARGUMENT_LIST),
				// parent, List.class);
				// TODO handle typeArguments

				List<String> qualified_alias_member = walk(
						commonTreeNode.getFirstChildWithType(CSharp4AST.QUALIFIED_ALIAS_MEMBER), parent, List.class);
				if (qualified_alias_member != null) nsTypeName.addAll(qualified_alias_member);

				for (int i = 1; i < node.getChildCount(); i++) {
					CommonTree child = (CommonTree) node.getChild(i);
					if (child.getType() == CSharp4AST.NAMESPACE_OR_TYPE_PART) {
						String nsPart = walk(child.getFirstChildWithType(CSharp4AST.IDENTIFIER), parent, String.class);
						nsTypeName.add(nsPart);
						// TODO TYPE_ARGUMENT_LIST
					}
				}
				return nsTypeName;
			case CSharp4AST.QUALIFIED_ALIAS_MEMBER:
				List<String> identifierList = new LinkedList<String>();
				// TODO
				return identifierList;

			case CSharp4AST.VARIABLE_DECLARATOR:
				return declaratorTransformer.transformMemberVariable(commonTreeNode, parent, declarations);

			case CSharp4AST.CONSTANT_DECLARATOR:
				// FIXME
				return declaratorTransformer.transformConstant(commonTreeNode, parent, declarations);

			case CSharp4AST.TYPE:
				List<CodeItem> _declarations = identifierResolver.resolveDeclarations(typeParameterDefinitions);
				try {
					return typeTransformator.transform(commonTreeNode, parent, _declarations, internalNamespaceStack,
							compilationUnit);
				} catch (TypeNotFoundException e) {
					// skip e.g. due to LOGGER.unsupported generic type
					return KDMElementFactory.createDatatype("LOGGER.unsupported: " + commonTreeNode.getChildren());
					// throw e; // TODO only for test purpose
				} catch (RuntimeException e) {
					// skip e.g. due to LOGGER.unsupported generic type
					return KDMElementFactory.createDatatype("LOGGER.unsupported: " + commonTreeNode.getChildren());
				}
			case CSharp4AST.IDENTIFIER:
				return node.getText();

			case CSharp4AST.ATTRIBUTE:
				// TODO
				// ATTRIBUTE_NAME;
				// POSITIONAL_ARGUMENT_LIST:
				break;
			case CSharp4AST.SIMPLE_NAME:
				String identifier = walk(commonTreeNode.getFirstChildWithType(CSharp4AST.IDENTIFIER), parent,
						String.class);
				return identifier;
			case CSharp4AST.MEMBER_ACCESS:
				List<String> accessList = new LinkedList<String>();

				Object accessPrefix = walk(commonTreeNode.getChild(0), parent);
				if (accessPrefix instanceof String) {
					accessList.add((String) accessPrefix);
				} else if (accessPrefix instanceof List) {
					accessList.addAll((Collection<? extends String>) accessPrefix);
				} else {
					System.err.println("LOGGER.unsupported: Cannot process member access of type => "
							+ accessPrefix.getClass());
				}

				String memberIdent = walk(commonTreeNode.getFirstChildWithType(CSharp4AST.IDENTIFIER), parent,
						String.class);
				accessList.add(memberIdent);

				// invocationPath.add(memberIdent);
				return accessList;
			default:
				LOG.warning("Skip; Process in next phase: " + commonTreeNode.getText());
				break;
		}
		return null;
	}

	private List<CodeItem> newNotNullLinkedList() {
		return ListUtil.<CodeItem> newNotNullLinkedList();
	}

	private AbstractCodeRelationship mapToRelation(final List<String> typeName, final CodeItem parent) {
		Datatype type = null;

		String extendsImplementsTypeName = typeName.get(0);
		if (typeName.size() == 1 && "object".equals(extendsImplementsTypeName)) {
			try {
				type = genericLanguageUnitCache.getDatatypeFromString(extendsImplementsTypeName);
			} catch (NotInCacheException e) {
				throw new IllegalArgumentException("The type " + extendsImplementsTypeName
						+ " is not contained in the LanguageUnit.");
			}
		} else {
			// resolve class
			try {
				type = (Datatype) identifierResolver.resolveType(typeName, Collections.<CodeItem> emptyList(),
						internalNamespaceStack, compilationUnit, parent);
				if (type.eContainer() == null) {
					// externalCodeModel.getCodeElement().add(type);
					throw new IllegalStateException("typeName: " + typeName + ",    type = " + type);
				}
			} catch (ClassCastException e) {
				// if resolveType returns a Namespace due to LOGGER.unsupported case
				throw new TypeNotFoundException(typeName.toString());
			}
		}
		// LOGGER.unsupported case: NamedEdge<R> : NamedEdge<S,T>
		if (parent == type) {
			throw new TypeNotFoundException("parent == type (" + typeName + ")");
		}

		// create KDM relation
		if (type instanceof InterfaceUnit) {
			Implements implementz = CodeFactory.eINSTANCE.createImplements();
			// have to be set from the corresponding class/interface/enum
			// implementz.setFrom(value);
			implementz.setTo(type);
			return implementz;
		} else if (type instanceof ClassUnit) {
			Extends extendz = CodeFactory.eINSTANCE.createExtends();
			// have to be set from the corresponding class/interface/enum
			// extendz.setFrom(value);
			extendz.setTo(type);
			return extendz;
		} else {
			// relationship is unknown at this point, so we just use an
			// arbitrary relation
			Extends extendz = CodeFactory.eINSTANCE.createExtends();
			// have to be set from the corresponding class/interface/enum
			// extendz.setFrom(value);
			extendz.setTo(type);
			return extendz;
		}
	}

	private AbstractCodeRelationship mapInterfaceToRelation(final List<String> typeName, final CodeItem parent) {
		Datatype type = (Datatype) identifierResolver.resolveType(typeName, Collections.<CodeItem> emptyList(),
				internalNamespaceStack, compilationUnit, parent);
		if (type.eContainer() == null) {
			// externalCodeModel.getCodeElement().add(type);
			throw new IllegalStateException("typeName: " + typeName + ",    type = " + type);
		}

		// LOGGER.unsupported case: NamedEdge<R> : NamedEdge<S,T>
		if (parent == type) {
			throw new TypeNotFoundException("parent == type (" + typeName + ")");
		}

		Extends extendz = CodeFactory.eINSTANCE.createExtends();
		// have to be set from the corresponding class/interface/enum
		// extendz.setFrom(value);
		extendz.setTo(type);
		return extendz;
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
			LOG.warning("Could not find type in cache: '" + predefinedTypeName + "'");
		}
	}

	@Override
	public List<CompilationUnit> getMappingResult() {
		return Arrays.asList(compilationUnit);
	}

}
