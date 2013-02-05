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

package mapping.code.extern;

import java.util.ArrayList;
import java.util.Collection;
import java.util.Deque;
import java.util.HashMap;
import java.util.LinkedList;
import java.util.List;
import java.util.Map;

import lang.csharp.CSharp4AST;
import mapping.IKDMMapper;
import mapping.code.extern.entity.DatatypeInfo;
import mapping.code.extern.entity.FieldInfo;
import mapping.code.extern.entity.MethodInfo;
import mapping.code.extern.entity.MethodInfo.ParameterInfo;
import mapping.code.extern.reference.ReferenceInfo;
import mapping.util.KeyStringHelper;
import mapping.util.TreeHelper;

import org.antlr.runtime.tree.CommonTree;
import org.antlr.runtime.tree.Tree;
import org.eclipse.gmt.modisco.omg.kdm.source.SourceFile;

import util.ListUtil;
import util.Logger;

public class DatatypeInfoMapper implements IKDMMapper<DatatypeInfo> {

	private static final Logger					LOGGER		= new Logger(DatatypeInfoMapper.class);
	private static final String					ABSTRACT	= "abstract";
	@Deprecated
	private List<DatatypeInfo>					datatypeInfos;
	private final Deque<String>					namespaceStack;

	private final Map<String, ReferenceInfo>	references	= new HashMap<String, ReferenceInfo>();

	public DatatypeInfoMapper() {
		this.namespaceStack = new LinkedList<String>();
	}

	@Override
	public void transform(final CommonTree ast) {
		LOGGER.info("DatatypeInfoMapper.transform..." + ast);
		namespaceStack.clear();

		this.datatypeInfos = new ArrayList<DatatypeInfo>(ast.getChildCount());

		try {
			for (int i = 0; i < ast.getChildCount(); i++) {
				walk(ast.getChild(i), null);
			}
		} catch (RuntimeException e) {
			LOGGER.error(e);
			throw e;
		}

		LOGGER.info("DONE: " + references.size() + " loaded.");
	}

	@SuppressWarnings("unchecked")
	private Object walk(final Tree node, final DatatypeInfo parent) {
		// an optional node that is not present in the current context, is null
		if (node == null) {
			return null;
		}

		DatatypeInfo newParent = parent;
		CommonTree commonTreeNode = (CommonTree) node;
		Tree modifiers;

		switch (node.getType()) {
			// case CsRewriteRulesParser.ATTRIBUTE:
			// System.out.println("ATTRIBUTE");
			// break;
			case CSharp4AST.NAMESPACE:
				CommonTree qid = (CommonTree) commonTreeNode.getFirstChildWithType(CSharp4AST.QUALIFIED_IDENTIFIER);

				Collection<String> namespaces = TreeHelper.treeListToStringList(qid.getChildren());
				namespaceStack.addAll(namespaces);

				walk(commonTreeNode.getFirstChildWithType(CSharp4AST.NAMESPACE_MEMBER_DECLARATIONS), parent);

				for (int i = 0; i < qid.getChildren().size(); i++)
					namespaceStack.removeLast();
				return null;
			case CSharp4AST.QUALIFIED_IDENTIFIER:
			case CSharp4AST.EXTERN_ALIAS_DIRECTIVES:
			case CSharp4AST.USING_DIRECTIVES:
			case CSharp4AST.NAMESPACE_MEMBER_DECLARATIONS:
			case CSharp4AST.ATTRIBUTES:
			case CSharp4AST.CLASS_MEMBER_DECLARATIONS:
			case CSharp4AST.INTERFACE_MEMBER_DECLARATIONS:
			case CSharp4AST.ENUM_MEMBER_DECLARATIONS:
			case CSharp4AST.STRUCT_MEMBER_DECLARATIONS:
			case CSharp4AST.CONST:
				for (int i = 0; i < commonTreeNode.getChildCount(); i++) {
					walk(commonTreeNode.getChild(i), parent);
				}
				return null;
			case CSharp4AST.CLASS:
				newParent = new DatatypeInfo("class");
				newParent.setNamespace(namespaceStack);
				datatypeInfos.add(newParent);

				String className = walk(commonTreeNode.getFirstChildWithType(CSharp4AST.IDENTIFIER), newParent,
						String.class);
				newParent.setName(className);

				LOGGER.fine("class: " + className);

				// modifiers
				modifiers = commonTreeNode.getFirstChildWithType(CSharp4AST.MODIFIERS);
				if (modifiers != null) {
					List<String> modifierNames = TreeHelper
							.treeListToStringList(((CommonTree) modifiers).getChildren());
					if (modifierNames.contains(ABSTRACT)) {
						newParent.setIsAbstract(Boolean.TRUE);
					}
				}

				setFullPath(parent, newParent);

				// must be invoked after setFullPath
				walk(commonTreeNode.getFirstChildWithType(CSharp4AST.CLASS_MEMBER_DECLARATIONS), newParent);

				addToReferences(newParent);
				return newParent;
			case CSharp4AST.INTERFACE:
				newParent = new DatatypeInfo("interface");
				newParent.setNamespace(namespaceStack);
				datatypeInfos.add(newParent);

				String interfaceName = walk(commonTreeNode.getFirstChildWithType(CSharp4AST.IDENTIFIER), newParent,
						String.class);
				newParent.setName(interfaceName);

				LOGGER.fine("interface: " + interfaceName);

				// transform(commonTreeNode.getFirstChildWithType(CSharp4AST.IMPLEMENTS),
				// newParent,
				// false);

				// modifiers
				modifiers = commonTreeNode.getFirstChildWithType(CSharp4AST.MODIFIERS);
				if (modifiers != null) {
					List<String> modifierNames = TreeHelper
							.treeListToStringList(((CommonTree) modifiers).getChildren());
					if (modifierNames.contains(ABSTRACT)) {
						newParent.setIsAbstract(Boolean.TRUE);
					}
				}

				setFullPath(parent, newParent);

				// must be invoked after setFullPath
				walk(commonTreeNode.getFirstChildWithType(CSharp4AST.INTERFACE_MEMBER_DECLARATIONS), newParent);

				addToReferences(newParent);
				return newParent;
			case CSharp4AST.ENUM:
				newParent = new DatatypeInfo("enum");
				newParent.setNamespace(namespaceStack);
				datatypeInfos.add(newParent);

				String enumName = walk(commonTreeNode.getFirstChildWithType(CSharp4AST.IDENTIFIER), newParent,
						String.class);
				newParent.setName(enumName);

				LOGGER.fine("enum: " + enumName);

				// modifiers
				modifiers = commonTreeNode.getFirstChildWithType(CSharp4AST.MODIFIERS);
				if (modifiers != null) {
					for (int i = 0; i < modifiers.getChildCount(); i++) {
						Tree modTree = modifiers.getChild(i);
						String modName = modTree.getText();

						if (ABSTRACT.equals(modName)) {
							newParent.setIsAbstract(Boolean.TRUE);
						}
					}
				}

				setFullPath(parent, newParent);

				// must be invoked after setFullPath
				walk(commonTreeNode.getFirstChildWithType(CSharp4AST.ENUM_MEMBER_DECLARATIONS), newParent);

				addToReferences(newParent);
				return newParent;
			case CSharp4AST.STRUCT:
				newParent = new DatatypeInfo("struct");
				newParent.setNamespace(namespaceStack);
				datatypeInfos.add(newParent);

				String structName = walk(commonTreeNode.getFirstChildWithType(CSharp4AST.IDENTIFIER), newParent,
						String.class);
				newParent.setName(structName);

				// IMPLEMENTS

				setFullPath(parent, newParent);

				walk(commonTreeNode.getFirstChildWithType(CSharp4AST.STRUCT_MEMBER_DECLARATIONS), newParent);

				addToReferences(newParent);

				LOGGER.fine("struct: " + newParent);
				return newParent;
			case CSharp4AST.DELEGATE:
				// see http://msdn.microsoft.com/de-de/library/900fyy8e%28v=vs.80%29.aspx
				newParent = new DatatypeInfo("delegate");
				newParent.setNamespace(namespaceStack);

				String delegateName = walk(commonTreeNode.getFirstChildWithType(CSharp4AST.IDENTIFIER), newParent,
						String.class);
				newParent.setName(delegateName);

				// TODO handle signature and generics

				setFullPath(parent, newParent);

				addToReferences(newParent);

				LOGGER.fine("delegate: " + newParent);
				break;
			case CSharp4AST.METHOD_DECL:
				MethodInfo methodInfo = new MethodInfo(parent);

				String returnType = walk(commonTreeNode.getFirstChildWithType(CSharp4AST.TYPE), parent, String.class);
				String methodName = walk(commonTreeNode.getFirstChildWithType(CSharp4AST.MEMBER_NAME), parent,
						String.class);
				List<ParameterInfo> formalParameters = walk(
						commonTreeNode.getFirstChildWithType(CSharp4AST.FORMAL_PARAMETER_LIST), parent, List.class);

				LOGGER.fine("method: " + methodName);

				methodInfo.setName(methodName);
				methodInfo.setReturnType(returnType);
				if (formalParameters != null) methodInfo.getParameters().addAll(formalParameters);
				parent.addMethodInfo(methodInfo);

				addToReferences(methodInfo);
				return methodInfo;
			case CSharp4AST.MEMBER_NAME:
				String typeName = walk(commonTreeNode.getFirstChildWithType(CSharp4AST.NAMESPACE_OR_TYPE_NAME), parent,
						String.class);
				return typeName;
			case CSharp4AST.FORMAL_PARAMETER_LIST:
				List<ParameterInfo> parameters = new LinkedList<ParameterInfo>();
				for (int i = 0; i < commonTreeNode.getChildCount(); i++) {
					Tree child = commonTreeNode.getChild(i);
					ParameterInfo parameter = walk(child, parent, ParameterInfo.class);
					parameters.add(parameter);
				}
				return parameters;
			case CSharp4AST.FIXED_PARAMETER:
				String fixedParamName = walk(commonTreeNode.getFirstChildWithType(CSharp4AST.IDENTIFIER), parent,
						String.class);

				String fixedParamType = walk(commonTreeNode.getFirstChildWithType(CSharp4AST.TYPE), parent,
						String.class);
				if (fixedParamType != null) { // if not __arglist
					fixedParamType = KeyStringHelper.normalize(fixedParamType);
				} else {
					fixedParamType = fixedParamName;
				}

				return new ParameterInfo(fixedParamType, fixedParamName);
			case CSharp4AST.PARAMETER_ARRAY:
				String paramArrayName = walk(commonTreeNode.getFirstChildWithType(CSharp4AST.IDENTIFIER), parent,
						String.class);

				String paramArrayType = walk(commonTreeNode.getFirstChildWithType(CSharp4AST.TYPE), parent,
						String.class);
				paramArrayType = KeyStringHelper.normalize(paramArrayType);

				return new ParameterInfo(paramArrayType, paramArrayName);
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
				FieldInfo propertyInfo = new FieldInfo(parent);

				String propertyName = walk(commonTreeNode.getFirstChildWithType(CSharp4AST.MEMBER_NAME), parent,
						String.class);
				if (propertyName != null) propertyInfo.setName(propertyName);

				String propertyType = walk(commonTreeNode.getFirstChildWithType(CSharp4AST.TYPE), parent, String.class);
				propertyInfo.setType(propertyType);

				parent.addFieldInfo(propertyInfo);

				addToReferences(propertyInfo);
				LOGGER.fine("LOAD PROPERTY: " + propertyInfo.getName());
				return propertyInfo;
			case CSharp4AST.VARIABLE_DECLARATOR:
				FieldInfo fieldInfo = new FieldInfo(parent);

				String fieldName = walk(commonTreeNode.getFirstChildWithType(CSharp4AST.IDENTIFIER), parent,
						String.class);
				if (fieldName != null) fieldInfo.setName(fieldName);

				String variableType = walk(commonTreeNode.getFirstChildWithType(CSharp4AST.TYPE), parent, String.class);
				fieldInfo.setType(variableType);

				parent.addFieldInfo(fieldInfo);

				addToReferences(fieldInfo);
				return fieldInfo;
			case CSharp4AST.ENUM_MEMBER_DECLARATION:
				LOGGER.warning("UNSUPPORTED: enum member declarations are not yet supported.");
				break;
			case CSharp4AST.TYPE:
				StringBuilder builder = new StringBuilder();
				Tree baseTypeNode = commonTreeNode.getChild(0);

				switch (baseTypeNode.getType()) {
					case CSharp4AST.NAMESPACE_OR_TYPE_NAME:
						String qualifiedBaseType = walk(baseTypeNode, parent, String.class);
						builder.append(qualifiedBaseType);
						break;
					default: // OBJECT, STRING, VOID, IDENTIFIER(dynamic), and primitive
						// types
						builder.append(baseTypeNode.getText());
						break;
				}

				for (int i = 1; i < commonTreeNode.getChildCount(); i++) {
					Tree typeExtension = commonTreeNode.getChild(i);
					// INTERR, rank_specifier, STAR
					switch (typeExtension.getType()) {
						case CSharp4AST.INTERR:
							LOGGER.warning("UNSUPPORTED: INTERR is not yet supported");
							break;
						case CSharp4AST.RANK_SPECIFIER:
							builder.append("[");
							int numCommas = typeExtension.getChildCount();
							do {
								builder.append(",");
							} while (numCommas-- > 0);
							builder.append("]");
							break;
						case CSharp4AST.STAR:
							builder.append("*");
							break;
						default:
							break;
					}
				}

				return builder.toString();
			case CSharp4AST.NAMESPACE_OR_TYPE_NAME:
				builder = new StringBuilder();

				String nsIdentifier = walk(commonTreeNode.getFirstChildWithType(CSharp4AST.IDENTIFIER), parent,
						String.class);
				if (nsIdentifier != null) builder.append(nsIdentifier);

				List<String> qualified_alias_member = walk(
						commonTreeNode.getFirstChildWithType(CSharp4AST.QUALIFIED_ALIAS_MEMBER), parent, List.class);
				if (qualified_alias_member != null) {
					for (String qam : qualified_alias_member) {
						builder.append(".");
						builder.append(qam);
					}
				}

				for (int i = 1; i < node.getChildCount(); i++) {
					CommonTree child = (CommonTree) node.getChild(i);
					if (child.getType() == CSharp4AST.NAMESPACE_OR_TYPE_PART) {
						String nsPart = walk(child.getFirstChildWithType(CSharp4AST.IDENTIFIER), parent, String.class);
						builder.append(".");
						builder.append(nsPart);
					}
				}
				return builder.toString();
			case CSharp4AST.IDENTIFIER:
				return node.getText();
			default:
				return null;
		}
		return null;
	}

	private void setFullPath(final DatatypeInfo parent, final DatatypeInfo datatype) {
		if (parent != null) {
			datatype.getFullPath().addAll(parent.getFullPath());
		} else {
			datatype.getFullPath().addAll(datatype.getNamespace());
		}
		datatype.getFullPath().add(datatype.getName());
		// System.out.println("DatatypeInfoMapper.setFullPath(): " +
		// datatype.getFullPath());
	}

	/**
	 * key=x.y.z
	 * 
	 * @param datatypeInfo
	 * @param parent
	 */
	private void addToReferences(final DatatypeInfo datatypeInfo) {
		String key = ListUtil.combine(datatypeInfo.getFullPath(), ".");
		references.put(key, datatypeInfo);
	}

	/**
	 * key=x.y.z.m:r(a,b,c)
	 * 
	 * @param methodInfo
	 */
	private void addToReferences(final MethodInfo methodInfo) {
		String key = ListUtil.combine(methodInfo.getOwner().getFullPath(), ".") + "." + methodInfo.getName() + "("
				+ ListUtil.combine(methodInfo.getParameters(), ",") + ")";
		references.put(key, methodInfo);
	}

	/**
	 * key=x.y.z
	 * 
	 * @param datatypeInfo
	 */
	private void addToReferences(final FieldInfo fieldInfo) {
		String key = ListUtil.combine(fieldInfo.getOwner().getFullPath(), ".") + "." + fieldInfo.getName();
		references.put(key, fieldInfo);
	}

	@SuppressWarnings("unchecked")
	private <T> T walk(final Tree node, final DatatypeInfo parent, final Class<T> class1) {
		return (T) walk(node, parent);
	}

	@Override
	public List<DatatypeInfo> getMappingResult() {
		return datatypeInfos;
	}

	@Override
	public void transform(final CommonTree node, final SourceFile sourceFile) {
		throw new UnsupportedOperationException();
	}

	public Map<String, ReferenceInfo> getReferences() {
		return references;
	}

}
