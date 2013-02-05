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

package util.generator;

import static org.junit.Assert.assertEquals;

import org.junit.Test;

/**
 * Extends {@link ASTGrammarGenerator} in order to test the protected method
 * <code>addImplicitListRules()</code>.
 * 
 * @author chw
 */
public class TestASTGrammarGeneratorTest extends ASTGrammarGenerator {

	@Test
	public void testAddImplicitListRules() {
		// 2 white spaces before first type, 1 white space between delimiter and next type
		String content = "testrule1:\n  type (',' type)*\n;";
		String actual = this.addImplicitListRules(content);
		String expected = "testrule1:\n  type (',' type)* -> type+\n;";
		assertEquals(expected, actual);
		// several white space between delimiter and next type
		content = "testrule2:\n\ttype (','   type)* ;";
		actual = this.addImplicitListRules(content);
		expected = "testrule2:\n\ttype (','   type)* -> type+ ;";
		assertEquals(expected, actual);
		// empty delimiter, tab implicitly as string
		content = "testrule3:\n	type (''   type)* ;";
		actual = this.addImplicitListRules(content);
		expected = "testrule3:\n	type (''   type)* -> type+ ;";
		assertEquals(expected, actual);
		// several white spaces after first type
		String equalContent = "testrule4:\n	expression  (','   expression)*";
		content = equalContent + " ;";
		actual = this.addImplicitListRules(content);
		expected = equalContent + " -> expression+ ;";
		assertEquals(expected, actual);
	}

}
