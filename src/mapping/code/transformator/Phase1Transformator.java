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
import java.util.LinkedList;
import java.util.List;

import lang.csharp.CSharp4AST;
import mapping.IKDMMapper;
import mapping.KDMElementFactory;
import mapping.code.namespace.NamespaceSearcher;
import mapping.code.namespace.NamespaceStack;
import mapping.code.resolver.NamespaceResolver;
import mapping.util.TreeHelper;

import org.antlr.runtime.tree.CommonTree;
import org.antlr.runtime.tree.Tree;
import org.eclipse.gmt.modisco.omg.kdm.code.ClassUnit;
import org.eclipse.gmt.modisco.omg.kdm.code.CodeItem;
import org.eclipse.gmt.modisco.omg.kdm.code.CodeModel;
import org.eclipse.gmt.modisco.omg.kdm.code.CompilationUnit;
import org.eclipse.gmt.modisco.omg.kdm.code.Datatype;
import org.eclipse.gmt.modisco.omg.kdm.code.EnumeratedType;
import org.eclipse.gmt.modisco.omg.kdm.code.InterfaceUnit;
import org.eclipse.gmt.modisco.omg.kdm.code.Namespace;
import org.eclipse.gmt.modisco.omg.kdm.code.TemplateParameter;
import org.eclipse.gmt.modisco.omg.kdm.code.TemplateUnit;
import org.eclipse.gmt.modisco.omg.kdm.source.SourceFile;
import org.eclipse.gmt.modisco.omg.kdm.source.SourceRef;

import util.ListUtil;
import util.Logger;

public class Phase1Transformator implements IKDMMapper<CompilationUnit> {

	private static final Logger		LOGGER	= new Logger(Phase1Transformator.class);

	private final Namespace			internalGlobalNamespace;
	private NamespaceStack			internalNamespaceStack;
	private CompilationUnit			compilationUnit;
	private final NamespaceSearcher	searcher;

	public Phase1Transformator(final CodeModel internalCodeModel) {
		this.internalGlobalNamespace = NamespaceSearcher.getGlobalNamespaceFrom(internalCodeModel);
		this.searcher = new NamespaceSearcher();
	}

	@Override
	public void transform(final CommonTree node) {
		throw new IllegalStateException();
	}

	@Override
	public void transform(final CommonTree node, final SourceFile sourceFile) {
		LOGGER.fine("Start transforming...");
		NamespaceResolver resolver = new NamespaceResolver();
		this.internalNamespaceStack = new NamespaceStack(internalGlobalNamespace, searcher, resolver);

		this.compilationUnit = KDMElementFactory.createCompilationUnit(sourceFile);

		for (int i = 0; i < node.getChildCount(); i++) {
			try {
				walk(node.getChild(i), internalGlobalNamespace);
			} catch (RuntimeException e) {
				LOGGER.error(e);
				throw e;
			}
		}

		LOGGER.fine("done.");
	}

	@SuppressWarnings("unchecked")
	private Object walk(final Tree node, final CodeItem parent) {
		// an optional node that is not present in the current context, is null
		if (node == null) {
			return null;
		}

		CommonTree commonTreeNode = (CommonTree) node;

		SourceRef sourceRef = TreeHelper.getCorrespondingSourceRef(commonTreeNode);

		switch (commonTreeNode.getType()) {
			case CSharp4AST.NAMESPACE:
				CommonTree qid = (CommonTree) commonTreeNode.getFirstChildWithType(CSharp4AST.QUALIFIED_IDENTIFIER);
				List<String> identifiers = TreeHelper.treeListToStringList(qid.getChildren());

				Namespace namespace = internalNamespaceStack.updateFromCurrentNamespace(identifiers);

				walk(commonTreeNode.getFirstChildWithType(CSharp4AST.NAMESPACE_MEMBER_DECLARATIONS), namespace);

				internalNamespaceStack.popCurrentNamespace(identifiers.size());
				break;
			case CSharp4AST.QUALIFIED_IDENTIFIER:
			case CSharp4AST.NAMESPACE_MEMBER_DECLARATIONS:
			case CSharp4AST.CLASS_MEMBER_DECLARATIONS:
			case CSharp4AST.INTERFACE_MEMBER_DECLARATIONS:
			case CSharp4AST.ENUM_MEMBER_DECLARATIONS:
			case CSharp4AST.STRUCT_MEMBER_DECLARATIONS:
				for (int i = 0; i < commonTreeNode.getChildCount(); i++) {
					walk(commonTreeNode.getChild(i), parent);
				}
				break;
			case CSharp4AST.TYPE_PARAMETERS:
				List<TemplateParameter> list = new LinkedList<TemplateParameter>();
				for (int i = 0; i < commonTreeNode.getChildCount(); i++) {
					TemplateParameter templateParameter = walk(commonTreeNode.getChild(i), parent,
							TemplateParameter.class);
					list.add(templateParameter);
				}
				return list;
			case CSharp4AST.CLASS:
				ClassUnit classUnit = KDMElementFactory.createClassUnit();
				classUnit.getSource().add(sourceRef);
				classUnit.setIsAbstract(Boolean.FALSE); // set default value

				List<String> namespacePath = ListUtil
						.codeItemsToNames(internalNamespaceStack.getCurrentNamespacePath());

				String className = walk(commonTreeNode.getFirstChildWithType(CSharp4AST.IDENTIFIER), classUnit,
						String.class);
				classUnit.setName(className);

				TemplateUnit templateUnit = handleGenerics(commonTreeNode, classUnit);
				Datatype unit = getMainUnit(templateUnit, classUnit);

				// each class unit is directly owned by the compilation unit, i.e. the source file
				if (parent instanceof Namespace) {
					compilationUnit.getCodeElement().add(unit);
					// furthermore, a class is related to a namespace
					internalNamespaceStack.getCurrentNamespace().getGroupedCode().add(unit);
				} else if (parent instanceof ClassUnit) {
					((ClassUnit) parent).getCodeElement().add(unit);
					// System.out.println("added to class: " + parent);
					namespacePath.add(parent.getName());
				} else {
					throw new IllegalStateException("Cannot add class to " + parent);
				}

				KDMElementFactory.addQualifiedAttributeTo(unit, namespacePath);
				if (templateUnit != null) {
					LOGGER.info("generic: " + KDMElementFactory.getQualifiedNameAttribute(templateUnit).getValue());
				} else {
					LOGGER.info("class: " + KDMElementFactory.getQualifiedNameAttribute(classUnit).getValue());
				}

				// classes can have nested types
				walk(commonTreeNode.getFirstChildWithType(CSharp4AST.CLASS_MEMBER_DECLARATIONS), classUnit);

				break;
			case CSharp4AST.STRUCT:
				ClassUnit structType = KDMElementFactory.createStruct();
				templateUnit = registerType(commonTreeNode, sourceRef, structType);
				if (templateUnit != null) {
					LOGGER.info("generic: " + KDMElementFactory.getQualifiedNameAttribute(templateUnit).getValue());
				} else {
					LOGGER.info("struct: " + KDMElementFactory.getQualifiedNameAttribute(structType).getValue());
				}

				// structs can have nested types
				walk(commonTreeNode.getFirstChildWithType(CSharp4AST.STRUCT_MEMBER_DECLARATIONS), structType);

				break;
			case CSharp4AST.INTERFACE:
				InterfaceUnit interfaceUnit = KDMElementFactory.createInterfaceUnit();
				templateUnit = registerType(commonTreeNode, sourceRef, interfaceUnit);
				if (templateUnit != null) {
					LOGGER.info("generic: " + KDMElementFactory.getQualifiedNameAttribute(templateUnit).getValue());
				} else {
					LOGGER.info("interface: " + KDMElementFactory.getQualifiedNameAttribute(interfaceUnit).getValue());
				}

				// walk(commonTreeNode.getFirstChildWithType(CSharp4AST.INTERFACE_MEMBER_DECLARATIONS),
				// interfaceUnit);
				break;
			case CSharp4AST.ENUM:
				EnumeratedType enumeratedType = KDMElementFactory.createEnum();
				registerType(commonTreeNode, sourceRef, enumeratedType);

				LOGGER.info("enum: " + KDMElementFactory.getQualifiedNameAttribute(enumeratedType).getValue());

				// walk(commonTreeNode.getFirstChildWithType(CSharp4AST.ENUM_MEMBER_DECLARATIONS),
				// enumeratedType);
				break;
			case CSharp4AST.DELEGATE:
				// see http://msdn.microsoft.com/de-de/library/900fyy8e%28v=vs.80%29.aspx
				Datatype delegateType = KDMElementFactory.createDelegate();
				// TODO handle signature
				registerType(commonTreeNode, sourceRef, delegateType);

				LOGGER.info("delegate: " + KDMElementFactory.getQualifiedNameAttribute(delegateType).getValue());
				break;
			case CSharp4AST.TYPE_PARAM:
				TemplateParameter templateParameter = KDMElementFactory.createTemplateParameter();
				// TODO ATTRIBUTES

				String name = walk(commonTreeNode.getFirstChildWithType(CSharp4AST.IDENTIFIER), templateParameter,
						String.class);
				templateParameter.setName(name);

				return templateParameter;
			case CSharp4AST.IDENTIFIER:
				return node.getText();
			default:
				break;
		}
		return null;
	}

	/**
	 * @param templateUnit
	 * @param dataType
	 * @return
	 */
	private Datatype getMainUnit(final TemplateUnit templateUnit, final Datatype dataType) {
		return (templateUnit != null) ? templateUnit : dataType;
	}

	private TemplateUnit handleGenerics(final CommonTree commonTreeNode, final CodeItem codeItem) {
		@SuppressWarnings("unchecked")
		List<TemplateParameter> typeParameters = walk(commonTreeNode.getFirstChildWithType(CSharp4AST.TYPE_PARAMETERS),
				codeItem, List.class);
		if (typeParameters != null) {
			String name = codeItem.getName() + "<" + getTypeParametersAsStringList(typeParameters) + ">";
			TemplateUnit templateUnit = KDMElementFactory.createTemplateUnit(name);
			templateUnit.getCodeElement().addAll(typeParameters);
			templateUnit.getCodeElement().add(codeItem);
			return templateUnit;
		}
		return null;
	}

	/**
	 * @param typeParameters
	 * @return
	 */
	private String getTypeParametersAsStringList(final List<TemplateParameter> typeParameters) {
		StringBuilder builder = new StringBuilder();
		for (TemplateParameter p : typeParameters) {
			builder.append(p.getName());
			builder.append(",");
		}
		return builder.toString();
	}

	private TemplateUnit registerType(final CommonTree commonTreeNode, final SourceRef sourceRef,
			final Datatype datatype) {
		datatype.getSource().add(sourceRef);

		String name = commonTreeNode.getFirstChildWithType(CSharp4AST.IDENTIFIER).getText();
		datatype.setName(name);

		TemplateUnit templateUnit = handleGenerics(commonTreeNode, datatype);

		Datatype unit = getMainUnit(templateUnit, datatype);

		List<String> namespacePath = ListUtil.codeItemsToNames(internalNamespaceStack.getCurrentNamespacePath());
		KDMElementFactory.addQualifiedAttributeTo(unit, namespacePath);
		// each datatype is directly owned by the compilation unit, i.e. the source file
		compilationUnit.getCodeElement().add(unit);
		// furthermore, a enum is related to a namespace
		internalNamespaceStack.getCurrentNamespace().getGroupedCode().add(unit);

		return templateUnit;
	}

	@SuppressWarnings("unchecked")
	public <T> T walk(final Tree node, final CodeItem parent, final Class<T> class1) {
		Object result = walk(node, parent);
		return ((T) result);
	}

	@Override
	public List<CompilationUnit> getMappingResult() {
		return Arrays.asList(compilationUnit);
	}

}
