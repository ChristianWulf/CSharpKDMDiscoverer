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

package mapping.code;

import java.util.List;

import org.eclipse.gmt.modisco.omg.kdm.action.ActionElement;
import org.eclipse.gmt.modisco.omg.kdm.action.ActionFactory;
import org.eclipse.gmt.modisco.omg.kdm.code.AbstractCodeElement;
import org.eclipse.gmt.modisco.omg.kdm.code.CodeModel;
import org.eclipse.gmt.modisco.omg.kdm.core.Element;
import org.eclipse.gmt.modisco.omg.kdm.kdm.Attribute;
import org.eclipse.gmt.modisco.omg.kdm.kdm.KdmFactory;
import org.junit.Assert;

public final class MoDiscoKDM {

	public static final String			CLASS_INSTANCE_CREATION	= "class instance creation";
	public static final String			EXPRESSION_STATEMENT	= "expression statement";
	public static final String			METHOD_INVOCATION		= "method invocation";
	private static final String			CONST_DECLARATION		= "const declaration";
	private static final ActionFactory	ACTION_FACTORY			= ActionFactory.eINSTANCE;
	public static final String			VARIABLE_ACCESS			= "variable access";
	public static final String			INVENTORYMODEL_NAME		= "source references";

	private MoDiscoKDM() {
		// utility class
	}

	public static void addModifierTo(final AbstractCodeElement entity, final String modName) {
		Attribute modifiersAttribute = getModifiersAttribute(entity);
		String value = modifiersAttribute.getValue();
		StringBuilder builder = new StringBuilder(value.length() + modName.length() + 1);
		builder.append(value);
		builder.append(modName);
		builder.append(" ");
		modifiersAttribute.setValue(builder.toString());
	}

	private static Attribute getModifiersAttribute(final Element entity) {
		for (Attribute attr : entity.getAttribute()) {
			if ("export".equals(attr.getTag())) {
				return attr;
			}
		}
		// if parent does not contain any export attribute, return a new one
		Attribute modifiersAttribute = KdmFactory.eINSTANCE.createAttribute();
		modifiersAttribute.setTag("export");
		modifiersAttribute.setValue("");
		entity.getAttribute().add(modifiersAttribute);
		return modifiersAttribute;
	}

	public static void createAndAddAccessModifierToElement(final String modName, final Element element) {
		// MoDisco's workaround for class access modifiers
		Attribute attribute = KdmFactory.eINSTANCE.createAttribute();
		attribute.setTag("export");
		attribute.setValue(modName);
		element.getAttribute().add(attribute);
	}

	public static ActionElement createVariableDeclaration() {
		ActionElement variableDecl = ACTION_FACTORY.createActionElement();
		variableDecl.setName("variable declaration");
		variableDecl.setKind("variable declaration");
		return variableDecl;
	}

	public static ActionElement createIfStatement() {
		ActionElement ifStatement = ACTION_FACTORY.createActionElement();
		ifStatement.setName("if");
		ifStatement.setKind("if");
		return ifStatement;
	}

	public static ActionElement createInfixExpression(final String name) {
		ActionElement infixExpr = ACTION_FACTORY.createActionElement();
		infixExpr.setName(name);
		infixExpr.setKind("infix expression");
		return infixExpr;
	}

	public static ActionElement createVariableAccess() {
		ActionElement varAccess = ACTION_FACTORY.createActionElement();
		varAccess.setName(VARIABLE_ACCESS);
		varAccess.setKind(VARIABLE_ACCESS);
		return varAccess;
	}

	public static ActionElement createExpressionStatement() {
		ActionElement exprStmt = ACTION_FACTORY.createActionElement();
		exprStmt.setName(EXPRESSION_STATEMENT);
		exprStmt.setKind(EXPRESSION_STATEMENT);
		return exprStmt;
	}

	public static ActionElement createAssignment(final String name) {
		ActionElement assignment = ACTION_FACTORY.createActionElement();
		assignment.setName(name);
		assignment.setKind("assignment");
		return assignment;
	}

	public static ActionElement createConditionalExpression() {
		ActionElement condExpr = ACTION_FACTORY.createActionElement();
		condExpr.setKind("conditional");
		condExpr.setName("conditional");
		return condExpr;
	}

	public static ActionElement createReturnStatement() {
		ActionElement returnStmt = ACTION_FACTORY.createActionElement();
		returnStmt.setKind("return");
		returnStmt.setName("return");
		return returnStmt;
	}

	public static ActionElement createThrowStatement() {
		ActionElement throwStmt = ACTION_FACTORY.createActionElement();
		throwStmt.setKind("throw");
		throwStmt.setName("throw");
		return throwStmt;
	}

	public static boolean isVariableAccess(final AbstractCodeElement eqLeft) {
		// Since only we are responsible for creating the model,
		// we can use fast pointer comparison instead of slow string comparison
		return (eqLeft.getName() == VARIABLE_ACCESS);
	}

	public static ActionElement createWhileStatement() {
		ActionElement whileStmt = ACTION_FACTORY.createActionElement();
		whileStmt.setKind("while");
		whileStmt.setName("while");
		return whileStmt;
	}

	public static ActionElement createPostfixExpression(final String opName) {
		ActionElement postfixExpr = ACTION_FACTORY.createActionElement();
		postfixExpr.setName(opName);
		postfixExpr.setKind("postfix expression");
		return postfixExpr;
	}

	public static ActionElement createDoWhileStatement() {
		ActionElement doWhileStmt = ACTION_FACTORY.createActionElement();
		doWhileStmt.setKind("do while");
		doWhileStmt.setName("do while");
		return doWhileStmt;
	}

	public static ActionElement createForStatement() {
		ActionElement forStmt = ACTION_FACTORY.createActionElement();
		forStmt.setKind("for");
		forStmt.setName("for");
		return forStmt;
	}

	/**
	 * MoDisco workaround: add right-hand to something that can own it
	 * 
	 * @param codeModel
	 * @param localVarAssignmentExpr
	 */
	public static void add(final CodeModel codeModel, final AbstractCodeElement localVarAssignmentExpr) {
		Assert.assertNull(localVarAssignmentExpr.eContainer());
		codeModel.getCodeElement().add(localVarAssignmentExpr);
	}

	public static ActionElement createMethodInvocation() {
		ActionElement methodInvocation = ACTION_FACTORY.createActionElement();
		methodInvocation.setKind(METHOD_INVOCATION);
		methodInvocation.setName(METHOD_INVOCATION);
		return methodInvocation;
	}

	public static ActionElement createConstDeclaration() {
		ActionElement variableDecl = ACTION_FACTORY.createActionElement();
		variableDecl.setName(CONST_DECLARATION);
		variableDecl.setKind(CONST_DECLARATION);
		return variableDecl;
	}

	public static ActionElement createClassInstanceCreationExpression() {
		ActionElement classInstCreation = ACTION_FACTORY.createActionElement();
		classInstCreation.setKind(CLASS_INSTANCE_CREATION);
		classInstCreation.setName(CLASS_INSTANCE_CREATION);
		return classInstCreation;
	}

	public static ActionElement createPrefixExpression(final String opName) {
		ActionElement postfixExpr = ACTION_FACTORY.createActionElement();
		postfixExpr.setName(opName);
		postfixExpr.setKind("prefix expression");
		return postfixExpr;
	}

	public static ActionElement createForeachStatement() {
		ActionElement forStmt = ACTION_FACTORY.createActionElement();
		forStmt.setKind("foreach");
		forStmt.setName("foreach");
		return forStmt;
	}

	public static void setModifiersTo(final List<String> modifierNames, final AbstractCodeElement entity) {
		if (modifierNames.isEmpty()) {
			addModifierTo(entity, "none");
		} else {
			for (String modName : modifierNames) {
				addModifierTo(entity, modName);
			}
		}
	}
}
