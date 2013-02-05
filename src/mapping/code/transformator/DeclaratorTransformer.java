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

import java.util.Deque;
import java.util.LinkedList;
import java.util.List;

import lang.csharp.CSharp4AST;
import mapping.KDMElementFactory;
import mapping.code.MoDiscoKDM;
import mapping.util.KDMChildHelper;
import mapping.util.TreeHelper;

import org.antlr.runtime.tree.CommonTree;
import org.antlr.runtime.tree.Tree;
import org.eclipse.gmt.modisco.omg.kdm.action.ActionElement;
import org.eclipse.gmt.modisco.omg.kdm.code.AbstractCodeElement;
import org.eclipse.gmt.modisco.omg.kdm.code.CodeItem;
import org.eclipse.gmt.modisco.omg.kdm.code.CodeModel;
import org.eclipse.gmt.modisco.omg.kdm.code.DataElement;
import org.eclipse.gmt.modisco.omg.kdm.code.Datatype;
import org.eclipse.gmt.modisco.omg.kdm.code.MethodUnit;
import org.eclipse.gmt.modisco.omg.kdm.code.StorableUnit;

import util.Logger;

public class DeclaratorTransformer {

	private static final Logger				LOGGER	= new Logger(DeclaratorTransformer.class);

	private static final String				STATIC	= "static";
	private final AbstractTransformator		transformator;
	private final ModifiersTransformator	modifiersTransformator;

	public DeclaratorTransformer(final AbstractTransformator transformator,
			final ModifiersTransformator modifiersTransformator, final CodeModel externalCodeModel) {
		this.transformator = transformator;
		this.modifiersTransformator = modifiersTransformator;
	}

	private <T> T walk(final Tree node, final CodeItem parent, final Class<T> class1) {
		return transformator.walk(node, parent, class1);
	}

	@SuppressWarnings("unchecked")
	public DataElement transformMemberVariable(final CommonTree node, final CodeItem parent,
			final Deque<List<CodeItem>> declarations) {
		CommonTree modifiers = (CommonTree) node.getFirstChildWithType(CSharp4AST.MODIFIERS);
		List<String> modifierNames = (modifiers != null) ? TreeHelper.treeListToStringList(modifiers.getChildren())
				: new LinkedList<String>();

		DataElement variable = KDMElementFactory.createMemberVariable(node.getParent().getType(),
				modifierNames.contains(STATIC));

		modifiersTransformator.transformVariableModifiers(variable, modifierNames);
		declarations.peek().add(variable);

		String identifier = walk(node.getFirstChildWithType(CSharp4AST.IDENTIFIER), variable, String.class);
		if (identifier != null) variable.setName(identifier);

		// memberUnit.setExt(value);
		// memberUnit.setKind(value);
		// memberUnit.setName(value);
		// memberUnit.setSize(value);
		// memberUnit.setType(value);

		((List<AbstractCodeElement>) KDMChildHelper.getChildrenFromCodeItem(parent)).add(variable);

		// TYPE
		assignType(node, variable);

		// markForPostProcessing(node.getFirstChildWithType(CSharp4AST.VARIABLE_INITIALIZER),
		// declarations, variable);

		return variable;
	}

	private void assignType(final CommonTree node, final DataElement dataElement) {
		Tree tree = node.getFirstChildWithType(CSharp4AST.TYPE);
		if (tree == null) {
			LOGGER.fatal("Cannot find TYPE in node's children: " + node.getText() + " -> " + node.getChildren());
			throw new AssertionError(tree);
		}
		Datatype datatype = walk(tree, dataElement, Datatype.class);
		dataElement.setType(datatype);
		if (datatype.eContainer() == null) {
			dataElement.getCodeElement().add(datatype);
		}
	}

	@SuppressWarnings("unchecked")
	public StorableUnit transformConstant(final CommonTree node, final CodeItem parent,
			final Deque<List<CodeItem>> declarations) {
		CommonTree modifiers = (CommonTree) node.getFirstChildWithType(CSharp4AST.MODIFIERS);
		List<String> modifierNames = (modifiers != null) ? TreeHelper.treeListToStringList(modifiers.getChildren())
				: new LinkedList<String>();

		// ActionElement constDecl = MoDiscoKDM.createConstDeclaration();

		// C# spec: page 33
		// "a constant value: a value that can be computed at compile-time"
		// "constants are considered static members"
		StorableUnit constVariable = KDMElementFactory.createConstantVariable();
		modifiersTransformator.transformVariableModifiers(constVariable, modifierNames);

		declarations.peek().add(constVariable);

		String identifier = walk(node.getFirstChildWithType(CSharp4AST.IDENTIFIER), constVariable, String.class);
		if (identifier != null) constVariable.setName(identifier);

		// add this declaration as soon as possible to the method so further statements can
		// recognize it
		((List<AbstractCodeElement>) KDMChildHelper.getChildrenFromCodeItem(parent)).add(constVariable);

		// constDecl.getCodeElement().add(constVariable);

		// TYPE
		assignType(node, constVariable);

		return constVariable;
	}

	public ActionElement transformLocalVariable(final CommonTree node, final CodeItem parent,
			final Deque<List<CodeItem>> declarations) {
		ActionElement variableDecl = MoDiscoKDM.createVariableDeclaration();
		// add this declaration as soon as possible to the method so further
		// statements
		// can recognize it
		((MethodUnit) parent).getCodeElement().add(variableDecl);

		String localVarName = walk(node.getFirstChildWithType(CSharp4AST.IDENTIFIER), parent, String.class);

		StorableUnit localVar = KDMElementFactory.createLocalVariable();
		// localVar.setExt(value);
		localVar.setName(localVarName);
		// localVar.setSize(value);

		declarations.peek().add(localVar);
		variableDecl.getCodeElement().add(localVar);

		assignType(node, localVar);

		// markForPostProcessing(node.getFirstChildWithType(CSharp4AST.LOCAL_VARIABLE_INITIALIZER),
		// declarations, localVar);

		return variableDecl;
	}

	@SuppressWarnings("unchecked")
	public CodeItem transformProperty(final CommonTree node, final CodeItem parent,
			final Deque<List<CodeItem>> declarations) {
		// MODIFIERS
		CommonTree modifiers = (CommonTree) node.getFirstChildWithType(CSharp4AST.MODIFIERS);
		List<String> modifierNames = (modifiers != null) ? TreeHelper.treeListToStringList(modifiers.getChildren())
				: new LinkedList<String>();

		DataElement propertyDecl = KDMElementFactory.createPropertyUnit(modifierNames.contains(STATIC));

		modifiersTransformator.transformVariableModifiers(propertyDecl, modifierNames);

		// ATTRIBUTES

		// MEMBER_NAME
		String propertyName = walk(node.getFirstChildWithType(CSharp4AST.MEMBER_NAME), propertyDecl, String.class);
		propertyDecl.setName(propertyName);

		declarations.peek().add(propertyDecl);

		((List<AbstractCodeElement>) KDMChildHelper.getChildrenFromCodeItem(parent)).add(propertyDecl);

		// TYPE
		assignType(node, propertyDecl);

		return propertyDecl;
	}

	@SuppressWarnings("unchecked")
	public DataElement transformEventProperty(final CommonTree node, final CodeItem parent,
			final Deque<List<CodeItem>> declarations) {
		// MODIFIERS
		CommonTree modifiers = (CommonTree) node.getFirstChildWithType(CSharp4AST.MODIFIERS);
		List<String> modifierNames = (modifiers != null) ? TreeHelper.treeListToStringList(modifiers.getChildren())
				: new LinkedList<String>();

		DataElement eventDecl = KDMElementFactory.createEventUnit();

		modifiersTransformator.transformVariableModifiers(eventDecl, modifierNames);

		// ATTRIBUTES

		// MEMBER_NAME
		String propertyName = walk(node.getFirstChildWithType(CSharp4AST.MEMBER_NAME), eventDecl, String.class);
		eventDecl.setName(propertyName);

		declarations.peek().add(eventDecl);

		((List<AbstractCodeElement>) KDMChildHelper.getChildrenFromCodeItem(parent)).add(eventDecl);

		// VARIABLE_DECLARATOR
		DataElement varDecl = walk(node.getFirstChildWithType(CSharp4AST.VARIABLE_DECLARATOR), eventDecl,
				DataElement.class);
		if (varDecl == null) {
			// TYPE
			assignType(node, eventDecl);
		}

		return eventDecl;
	}

	@SuppressWarnings("unchecked")
	public DataElement transformIndexer(final CommonTree node, final CodeItem parent,
			final Deque<List<CodeItem>> declarations) {
		// MODIFIERS
		CommonTree modifiers = (CommonTree) node.getFirstChildWithType(CSharp4AST.MODIFIERS);
		List<String> modifierNames = (modifiers != null) ? TreeHelper.treeListToStringList(modifiers.getChildren())
				: new LinkedList<String>();

		DataElement indexerDecl = KDMElementFactory.createIndexerUnit();

		modifiersTransformator.transformVariableModifiers(indexerDecl, modifierNames);

		// ATTRIBUTES

		// MEMBER_NAME
		String propertyName = walk(node.getFirstChildWithType(CSharp4AST.MEMBER_NAME), indexerDecl, String.class);
		indexerDecl.setName(propertyName);

		declarations.peek().add(indexerDecl);

		((List<AbstractCodeElement>) KDMChildHelper.getChildrenFromCodeItem(parent)).add(indexerDecl);

		// TYPE
		assignType(node, indexerDecl);

		// TODO FORMAL_PARAMETER_LIST

		return indexerDecl;
	}

	/**
	 * @param commonTreeNode
	 * @param parent
	 * @param declarations
	 * @return
	 */
	public Object transformEventInterface(final CommonTree commonTreeNode, final CodeItem parent, final Deque<List<CodeItem>> declarations) {
		DataElement eventUnit = KDMElementFactory.createEventUnit();

		String identifier = walk(commonTreeNode.getFirstChildWithType(CSharp4AST.IDENTIFIER), eventUnit, String.class);
		if (identifier != null) eventUnit.setName(identifier);

		((List<AbstractCodeElement>) KDMChildHelper.getChildrenFromCodeItem(parent)).add(eventUnit);

		// TYPE
		assignType(commonTreeNode, eventUnit);

		// markForPostProcessing(node.getFirstChildWithType(CSharp4AST.VARIABLE_INITIALIZER),
		// declarations, variable);

		return eventUnit;
	}
}
