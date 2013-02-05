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

import org.antlr.runtime.tree.CommonTree;
import org.antlr.runtime.tree.Tree;
import org.eclipse.gmt.modisco.omg.kdm.code.CodeItem;
import org.eclipse.gmt.modisco.omg.kdm.code.Datatype;
import org.eclipse.gmt.modisco.omg.kdm.code.ParameterKind;
import org.eclipse.gmt.modisco.omg.kdm.code.ParameterUnit;
import org.eclipse.gmt.modisco.omg.kdm.code.PrimitiveType;
import org.eclipse.gmt.modisco.omg.kdm.code.TemplateParameter;
import org.junit.Assert;

import util.Logger;

public class ParameterTransformer {

	private static final Logger			LOGGER	= new Logger(ParameterTransformer.class);

	private final AbstractTransformator	transformator;

	public ParameterTransformer(final AbstractTransformator transformator) {
		this.transformator = transformator;
	}

	private <T> T walk(final Tree node, final CodeItem parent, final Class<T> class1) {
		return transformator.walk(node, parent, class1);
	}

	public ParameterUnit transformParameterArray(final CommonTree commonTreeNode,
			final CodeItem parent, final Deque<List<CodeItem>> declarations) {
		// walk(commonTreeNode.getFirstChildWithType(CSharp4AST.ATTRIBUTES), parent);
		// TODO use KDM ArrayType instead of Datatype
		Datatype paramArrayType = walk(commonTreeNode.getFirstChildWithType(CSharp4AST.TYPE),
				parent, Datatype.class);
		String paramArrayName = walk(commonTreeNode.getFirstChildWithType(CSharp4AST.IDENTIFIER),
				parent, String.class);
		Assert.assertNotNull(paramArrayName);

		ParameterUnit parameterArrayUnit = KDMElementFactory.createParameterUnit(paramArrayName);
		declarations.peek().add(parameterArrayUnit);
		parameterArrayUnit.setKind(ParameterKind.BY_REFERENCE);
		// parameterUnit.setPos(value); // is set in case FORMAL_PARAMETER_LIST
		// parameterUnit.setSize(value);
		parameterArrayUnit.setType(paramArrayType);
		parameterArrayUnit.getCodeElement().add(paramArrayType);
		return parameterArrayUnit;
	}

	public ParameterUnit transformFixedParameter(final CommonTree commonTreeNode,
			final CodeItem parent, final Deque<List<CodeItem>> declarations) {
		String paramName = walk(commonTreeNode.getFirstChildWithType(CSharp4AST.IDENTIFIER),
				parent, String.class);
		Assert.assertNotNull(paramName);

		Datatype paramType = walk(commonTreeNode.getFirstChildWithType(CSharp4AST.TYPE), parent,
				Datatype.class);
		if (paramType == null) { // if e.g. __arglist
			paramType = KDMElementFactory.createDatatype(paramName);
		}

		ParameterKind paramKind;
		if (paramType instanceof PrimitiveType) {
			paramKind = ParameterKind.BY_VALUE;
		} else {
			paramKind = ParameterKind.UNKNOWN;
		}

		// zero or exactly one of the following
		Tree paramModifier = commonTreeNode.getFirstChildWithType(CSharp4AST.PARAMETER_MODIFIER);
		// TODO set default kind according to primitive and complex types
		if (paramModifier != null) {
			switch (paramModifier.getType()) {
				case CSharp4AST.REF:
					paramKind = ParameterKind.BY_REFERENCE;
					break;
				case CSharp4AST.OUT:
					paramKind = ParameterKind.BY_REFERENCE;
					break;
				case CSharp4AST.THIS:
					paramKind = ParameterKind.UNKNOWN;
					break;
				default:
					paramKind = ParameterKind.UNKNOWN;
					LOGGER.warning("UNSUPPORTED: The parameter modifier '"
							+ paramModifier.getText() + "' is not yet supported.");
					break;
			}
		}

		ParameterUnit parameterUnit = KDMElementFactory.createParameterUnit(paramName);
		declarations.peek().add(parameterUnit);
		parameterUnit.setKind(paramKind);
		// parameterUnit.setPos(value); // is set in case FORMAL_PARAMETER_LIST
		// parameterUnit.setSize(value);
		parameterUnit.setType(paramType);
		if (paramType.eContainer() == null) {
			parameterUnit.getCodeElement().add(paramType);
		}

		return parameterUnit;
	}

	public TemplateParameter transformTypeParameter(final CommonTree commonTreeNode, final CodeItem parent,
			final Deque<List<CodeItem>> declarations) {
		String typeParamIdent = walk(
				commonTreeNode.getFirstChildWithType(CSharp4AST.IDENTIFIER), parent,
				String.class);
		TemplateParameter typeParam = KDMElementFactory.createTypeParameter(typeParamIdent);

		declarations.peek().add(typeParam);
		return typeParam;
	}

}
