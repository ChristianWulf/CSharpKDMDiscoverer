/***************************************************************************
 * Copyright 2012 by
 * + Christian-Albrechts-University of Kiel
 * + Department of Computer Science
 * + Software Engineering Group
 * and others.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 ***************************************************************************/

package mapping.code.transformator;

import java.util.List;

import lang.csharp.CSharp4AST;
import mapping.KDMElementFactory;
import mapping.code.extern.TypeNotFoundException;
import mapping.code.namespace.NamespaceStack;

import org.antlr.runtime.tree.CommonTree;
import org.antlr.runtime.tree.Tree;
import org.eclipse.gmt.modisco.omg.kdm.code.AbstractCodeElement;
import org.eclipse.gmt.modisco.omg.kdm.code.ClassUnit;
import org.eclipse.gmt.modisco.omg.kdm.code.CodeItem;
import org.eclipse.gmt.modisco.omg.kdm.code.Datatype;
import org.eclipse.gmt.modisco.omg.kdm.code.EnumeratedType;
import org.eclipse.gmt.modisco.omg.kdm.code.InterfaceUnit;
import org.eclipse.gmt.modisco.omg.kdm.code.MethodUnit;
import org.eclipse.gmt.modisco.omg.kdm.code.ParameterUnit;
import org.eclipse.gmt.modisco.omg.kdm.code.Signature;
import org.eclipse.gmt.modisco.omg.kdm.code.TemplateParameter;
import org.eclipse.gmt.modisco.omg.kdm.code.TemplateUnit;

import util.ListUtil;

public class MethodTransformator {

	private final AbstractTransformator	transformator;
	private final ModifiersTransformator	modifiersTransformator;

	public MethodTransformator(final AbstractTransformator transformator, final ModifiersTransformator modifiersTransformator) {
		this.transformator = transformator;
		this.modifiersTransformator = modifiersTransformator;
	}

	public CodeItem transform(final CommonTree commonTreeNode, final AbstractCodeElement parent,
			final NamespaceStack internalNamespaceStack) {
		MethodUnit methodUnit = KDMElementFactory.createMethodUnit();
		CodeItem result = methodUnit; // result is MethodUnit or TemplateUnit

		Signature methodSignature = KDMElementFactory.createSignature();
		methodUnit.setType(methodSignature);
		methodUnit.getCodeElement().add(methodSignature);

		// modifiers
		CommonTree modifiers = (CommonTree) commonTreeNode.getFirstChildWithType(CSharp4AST.MODIFIERS);
		modifiersTransformator.transformMethodModifier(parent, methodUnit, modifiers);

		String methodName = walk(commonTreeNode.getFirstChildWithType(CSharp4AST.MEMBER_NAME),
				methodUnit, String.class);
		methodUnit.setName(methodName);
		methodSignature.setName(methodName);

		@SuppressWarnings("unchecked")
		List<TemplateParameter> typeParams = walk(
				commonTreeNode.getFirstChildWithType(CSharp4AST.TYPE_PARAMETERS), methodUnit,
				List.class);
		if (typeParams != null) {
			TemplateUnit tunit = KDMElementFactory.createTemplateUnit(methodName + "<"
					+ ListUtil.codeItemsToNames(typeParams) + ">");
			tunit.getCodeElement().addAll(typeParams); // first type parameters (see kdm spec)
			tunit.getCodeElement().add(methodUnit);
			result = tunit;
		}

		if (parent instanceof InterfaceUnit) {
			((InterfaceUnit) parent).getCodeElement().add(result);
		} else if (parent instanceof ClassUnit) {
			((ClassUnit) parent).getCodeElement().add(result);
		} else if (parent instanceof EnumeratedType) {
			((EnumeratedType) parent).getCodeElement().add(result);
		}
		//		System.out.println("ADDED (template) method '" + methodUnit.getName() + "' to " + parent.getName());

		Datatype returnType = walk(commonTreeNode.getFirstChildWithType(CSharp4AST.TYPE),
				methodUnit, Datatype.class);

		@SuppressWarnings("unchecked")
		List<ParameterUnit> formalParameters = walk(
				commonTreeNode.getFirstChildWithType(CSharp4AST.FORMAL_PARAMETER_LIST), methodUnit,
				List.class);

		// walk(commonTreeNode
		// .getFirstChildWithType(CSharp4AST.TYPE_PARAMETER_CONSTRAINTS_CLAUSES),
		// methodUnit);

		// do not transform actions at this point because there can be references to members that
		// are not yet defined
		// PostProcessEntity ppEntity = new PostProcessEntity();
		// ppEntity.setNamespacePath(internalNamespaceStack.getCurrentNamespacePath());
		// ppEntity.setTree(commonTreeNode.getFirstChildWithType(CSharp4AST.BLOCK));
		// ppEntity.setDeclarations(declarations);
		// methodBodies.put(methodUnit, ppEntity);

		// return type
		ParameterUnit returnUnit = KDMElementFactory.createReturnType();
		// KDM Spec.: Return parameter of a signature does not have a pos attribute.
		returnUnit.setType(returnType);
		if (returnType.eContainer() == null) returnUnit.getCodeElement().add(returnType);

		methodSignature.getParameterUnit().add(returnUnit);
		if (formalParameters != null) methodSignature.getParameterUnit().addAll(formalParameters);

		return result;
	}

	private <T> T walk(final Tree node, final CodeItem parent, final Class<T> class1)
			throws TypeNotFoundException {
		return transformator.walk(node, parent, class1);
	}

	// public void postProcessMethodBodies(final NamespaceStack internalNamespaceStack,
	// final Deque<List<CodeItem>> declarations) {
	// for (MethodUnit m : methodBodies.keySet()) {
	// PostProcessEntity savedState = methodBodies.get(m);
	// // restore namespace hierarchy for this method (owning type)
	// internalNamespaceStack.setCurrentNamespaceStack(savedState.getNamespacePath());
	// // restore declarations
	// declarations.clear();
	// declarations.addAll(savedState.getDeclarations());
	// // process body
	// BlockUnit methodBody = walk(savedState.getTree(), m, BlockUnit.class);
	// methodBody.setKind("method body");
	// m.getCodeElement().add(methodBody);
	// }
	// methodBodies.clear(); // free for gc
	// }

}
