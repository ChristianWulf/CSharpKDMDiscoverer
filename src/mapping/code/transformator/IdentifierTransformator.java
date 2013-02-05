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

import java.util.LinkedList;
import java.util.List;

import lang.csharp.CSharp4AST;

import org.antlr.runtime.tree.BaseTree;
import org.antlr.runtime.tree.CommonTree;
import org.antlr.runtime.tree.Tree;
import org.eclipse.gmt.modisco.omg.kdm.code.CodeItem;

public class IdentifierTransformator {

	private final Phase3Transformator	transformator;

	public IdentifierTransformator(final Phase3Transformator transformator) {
		this.transformator = transformator;
	}

	public List<String>  transformNamespaceOrTypename(final BaseTree node, final CodeItem parent) {
		List<String> nsTypeName = new LinkedList<String>();

		Object result = walk(node.getFirstChildWithType(CSharp4AST.IDENTIFIER), parent, Object.class);
		if (result != null) nsTypeName.add((String) result);

		// List<Datatype> typeArguments = walk(
		// commonTreeNode.getFirstChildWithType(CSharp4AST.TYPE_ARGUMENT_LIST),
		// parent, List.class);
		// TODO handle typeArguments

		@SuppressWarnings("unchecked")
		List<String> qualified_alias_member = walk(
				node.getFirstChildWithType(CSharp4AST.QUALIFIED_ALIAS_MEMBER), parent, List.class);
		if (qualified_alias_member != null) nsTypeName.addAll(qualified_alias_member);

		for (int i = 1; i < node.getChildCount(); i++) {
			CommonTree child = (CommonTree) node.getChild(i);
			if (child.getType() == CSharp4AST.NAMESPACE_OR_TYPE_PART) {
				String nsPart = walk(child.getFirstChildWithType(CSharp4AST.IDENTIFIER), parent,
						String.class);
				nsTypeName.add(nsPart);
				// TODO TYPE_ARGUMENT_LIST
			}
		}
		return nsTypeName;
	}

	private <T> T walk(final Tree node, final CodeItem parent, final Class<T> clazz) {
		return transformator.walk(node, parent, clazz);
	}

}
