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

import java.util.Deque;
import java.util.List;

import lang.csharp.CSharp4AST;
import mapping.KDMElementFactory;
import mapping.code.MoDiscoKDM;

import org.antlr.runtime.tree.CommonTree;
import org.antlr.runtime.tree.Tree;
import org.eclipse.gmt.modisco.omg.kdm.action.ActionElement;
import org.eclipse.gmt.modisco.omg.kdm.code.AbstractCodeElement;
import org.eclipse.gmt.modisco.omg.kdm.code.CodeItem;
import org.eclipse.gmt.modisco.omg.kdm.code.DataElement;

import util.ListUtil;

public class LoopTransformator {

	private final Phase3Transformator transformator;

	public LoopTransformator(final Phase3Transformator transformator) {
		this.transformator = transformator;
	}

	private <T> T walk(final Tree node, final CodeItem parent, final Class<T> clazz) {
		return transformator.walk(node, parent, clazz);
	}

	public ActionElement transformWhile(final CommonTree commonTreeNode,
			final CodeItem parent, final Deque<List<CodeItem>> declarations) {
		ActionElement whileStatement = MoDiscoKDM.createWhileStatement();
		declarations.push(ListUtil.<CodeItem> newNotNullLinkedList());
		try {
			AbstractCodeElement whileCondition = walk(
					commonTreeNode.getFirstChildWithType(CSharp4AST.CONDITION), parent,
					AbstractCodeElement.class);
			if (whileCondition instanceof DataElement) {
				whileCondition = KDMElementFactory.createReadAccess(whileStatement,
						(DataElement) whileCondition);
			}
			whileStatement.getCodeElement().add(whileCondition);

			ActionElement whileBody = walk(
					commonTreeNode.getFirstChildWithType(CSharp4AST.LOOP_BODY), parent,
					ActionElement.class);
			if (whileBody == null) {
				whileBody = KDMElementFactory.createBlockUnit();
				whileBody.setName("UNSUPPORTED while-loop body");
			}
			whileStatement.getCodeElement().add(whileBody);
		} finally {
			declarations.pop();
		}
		return whileStatement;
	}

	public ActionElement transformDoWhile(final CommonTree commonTreeNode,
			final CodeItem parent, final Deque<List<CodeItem>> declarations) {
		ActionElement doWhileStatement = MoDiscoKDM.createDoWhileStatement();
		declarations.push(ListUtil.<CodeItem> newNotNullLinkedList());
		try {
			AbstractCodeElement doWhileCondition = walk(
					commonTreeNode.getFirstChildWithType(CSharp4AST.CONDITION), parent,
					AbstractCodeElement.class);
			if (doWhileCondition == null) {
				throw new ExpressionNotSupported("doWhileCondition == null");
			} else if (doWhileCondition instanceof DataElement) {
				doWhileCondition = KDMElementFactory.createReadAccess(doWhileStatement,
						(DataElement) doWhileCondition);
			}
			doWhileStatement.getCodeElement().add(doWhileCondition);

			ActionElement doWhileBody = walk(
					commonTreeNode.getFirstChildWithType(CSharp4AST.LOOP_BODY), parent,
					ActionElement.class);
			if (doWhileBody == null) {
				doWhileBody = KDMElementFactory.createBlockUnit();
				doWhileBody.setName("UNSUPPORTED do-while-loop body");
			}
			doWhileStatement.getCodeElement().add(doWhileBody);
		} finally {
			declarations.pop();
		}
		return doWhileStatement;
	}

	public ActionElement transformFor(final CommonTree commonTreeNode,
			final CodeItem parent, final Deque<List<CodeItem>> declarations) {
		ActionElement forStatement = MoDiscoKDM.createForStatement();
		declarations.push(ListUtil.<CodeItem> newNotNullLinkedList());
		try {
			AbstractCodeElement for_initializer = walk(
					commonTreeNode.getFirstChildWithType(CSharp4AST.FOR_INITIALIZER),
					parent, AbstractCodeElement.class);
			if (for_initializer == null) {
				throw new ExpressionNotSupported("for_initializer == null");
			} else if (for_initializer instanceof DataElement) {
				for_initializer = KDMElementFactory.createReadAccess(forStatement,
						(DataElement) for_initializer);
			}
			forStatement.getCodeElement().add(for_initializer);

			AbstractCodeElement for_condition = walk(
					commonTreeNode.getFirstChildWithType(CSharp4AST.CONDITION), parent,
					AbstractCodeElement.class);
			if (for_condition == null) {
				throw new ExpressionNotSupported("for_condition == null");
			} else if (for_condition instanceof DataElement) {
				for_condition = KDMElementFactory.createReadAccess(forStatement,
						(DataElement) for_condition);
			}
			forStatement.getCodeElement().add(for_condition);

			AbstractCodeElement for_iterator = walk(
					commonTreeNode.getFirstChildWithType(CSharp4AST.FOR_ITERATOR),
					parent, AbstractCodeElement.class);
			if (for_iterator == null) {
				throw new ExpressionNotSupported("for_iterator == null");
			} else if (for_iterator instanceof DataElement) {
				for_iterator = KDMElementFactory.createReadAccess(forStatement,
						(DataElement) for_iterator);
			}
			forStatement.getCodeElement().add(for_iterator);

			ActionElement block = walk(
					commonTreeNode.getFirstChildWithType(CSharp4AST.LOOP_BODY), parent,
					ActionElement.class);
			if (block == null) {
				block = KDMElementFactory.createBlockUnit();
				block.setName("UNSSUPORTED for-loop body");
			}
			forStatement.getCodeElement().add(block);
		} finally {
			declarations.pop();
		}
		return forStatement;
	}

	public ActionElement transformForeach(final CommonTree commonTreeNode,
			final CodeItem parent, final Deque<List<CodeItem>> declarations) {
		ActionElement foreachStatement = MoDiscoKDM.createForeachStatement();
		declarations.push(ListUtil.<CodeItem> newNotNullLinkedList());
		try {
			// TODO
			// local_variable_type
			// IDENTIFIER
			// ^(IN expression)
			// embedded_statement

		} finally {
			declarations.pop();
		}
		return foreachStatement;
	}

}
