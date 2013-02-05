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

import mapping.KDMElementFactory;
import mapping.code.MoDiscoKDM;
import mapping.code.namespace.NamespaceStack;
import mapping.code.resolver.IdentifierResolver;

import org.antlr.runtime.tree.Tree;
import org.eclipse.gmt.modisco.omg.kdm.action.ActionElement;
import org.eclipse.gmt.modisco.omg.kdm.code.AbstractCodeElement;
import org.eclipse.gmt.modisco.omg.kdm.code.CodeItem;
import org.eclipse.gmt.modisco.omg.kdm.code.CompilationUnit;
import org.eclipse.gmt.modisco.omg.kdm.code.DataElement;

public class OperatorTransformator {

	private final Phase3Transformator transformator;
	private final IdentifierResolver identifierResolver;

	public OperatorTransformator(final Phase3Transformator transformator,
			final IdentifierResolver identifierResolver) {
		this.transformator = transformator;
		this.identifierResolver = identifierResolver;
	}

	public ActionElement transformBinaryOp(final Tree node,
			final CodeItem parent, final String opName) {
		try {
			ActionElement opExpr = MoDiscoKDM.createInfixExpression(opName);

			AbstractCodeElement eqLeft = walk(node.getChild(0), parent,
					AbstractCodeElement.class);
			AbstractCodeElement eqRight = walk(node.getChild(1), parent,
					AbstractCodeElement.class);

			AbstractCodeElement leftHand = eqLeft;
			if (leftHand instanceof ActionElement) {
				ActionElement action = (ActionElement) leftHand;
				if (MoDiscoKDM.METHOD_INVOCATION.equals(action.getName())) {
					throw new ExpressionNotSupported(
							"Binary operation with method invocation is not yet supported.");
				}
			}

			if (leftHand == null) {
				leftHand = KDMElementFactory.createActionElementFallback();
				// } else if (!(leftHand instanceof ValueElement)) {
			} else if (leftHand instanceof DataElement) {
				leftHand = KDMElementFactory.createReadAccess(opExpr,
						(DataElement) leftHand);
			}

			if (!(leftHand instanceof ActionElement)) {
				leftHand = KDMElementFactory.createActionElementFallback();
			}
			opExpr.getCodeElement().add(leftHand);

			AbstractCodeElement rightHand = eqRight;
			if (rightHand == null) {
				rightHand = KDMElementFactory.createActionElementFallback();
				// } else if (!(rightHand instanceof ValueElement)) {
			} else if (rightHand instanceof DataElement) {
				rightHand = KDMElementFactory.createReadAccess(opExpr,
						(DataElement) rightHand);
			}

			if (!(rightHand instanceof ActionElement)) {
				rightHand = KDMElementFactory.createActionElementFallback();
			}
			opExpr.getCodeElement().add(rightHand);

			return opExpr;
		} catch (RuntimeException e) {
			throw new ExpressionNotSupported(e.getLocalizedMessage());
		}
	}

	private <T> T walk(final Tree node, final CodeItem parent,
			final Class<T> clazz) {
		return transformator.walk(node, parent, clazz);
	}

	public ActionElement transformUnaryOp(final Tree node,
			final CodeItem parent, final String opName,
			final Deque<List<CodeItem>> declarations,
			final NamespaceStack internalNamespaceStack,
			final CompilationUnit compilationUnit) {
		try {
			ActionElement opExpr = MoDiscoKDM.createPostfixExpression(opName);

			DataElement itemToBeRead = null;

			Object result = walk(node.getChild(0), parent, Object.class);
			if (result == null) {
				result = KDMElementFactory.createActionElementFallback();
			} else if (result instanceof DataElement) {
				itemToBeRead = (DataElement) result;
			} else if (result instanceof String) {
				List<CodeItem> _declarations = identifierResolver
						.resolveDeclarations(declarations);
				itemToBeRead = (DataElement) identifierResolver
						.resolveIdentifier((String) result, _declarations,
								internalNamespaceStack, compilationUnit, parent);
			}
			if (itemToBeRead == null) {
				itemToBeRead = KDMElementFactory.createDummyDataElement(result
						.toString());
				opExpr.getCodeElement().add(itemToBeRead);
			}
			// TODO also write access, e.g. i++

			ActionElement baseAccess = KDMElementFactory.createReadAccess(
					opExpr, itemToBeRead);

			opExpr.getCodeElement().add(baseAccess);
			return opExpr;
		} catch (RuntimeException e) {
			throw new ExpressionNotSupported(e.getLocalizedMessage());
		}
	}

	public ActionElement transformPrefixUnaryOp(final Tree node,
			final CodeItem parent, final String opName) {
		try {
			ActionElement opExpr = MoDiscoKDM.createPrefixExpression(opName);

			AbstractCodeElement base = walk(node.getChild(0), parent,
					AbstractCodeElement.class);
			if (base == null) {
				base = KDMElementFactory.createActionElementFallback();
			}
			if (!(base instanceof ActionElement)) {
				ActionElement reads = KDMElementFactory.createReadAccess(
						opExpr, (DataElement) base);
				opExpr.getCodeElement().add(reads);
				// TODO also write access, e.g. ++i
			} else {
				opExpr.getCodeElement().add(base);
			}

			return opExpr;
		} catch (RuntimeException e) {
			throw new ExpressionNotSupported(e.getLocalizedMessage());
		}
	}

}
