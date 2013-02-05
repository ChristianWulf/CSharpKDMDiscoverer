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

import java.io.IOException;
import java.util.regex.Matcher;
import java.util.regex.Pattern;

import util.FileAccess;

public class ASTGrammarGenerator {

	private static final String	IDENTIFIER	= "(\\w*)";
	private static final String	REGEX		= "(" + IDENTIFIER
			+ "\\s*\\('.*?'\\s+\\2\\)\\*)(\\s*?;)";
	private static final String	REPLACEMENT	= "$1 \\-> $2\\+$3";
	private final Pattern		pattern;

	public ASTGrammarGenerator() {
		pattern = Pattern.compile(REGEX);
	}

	public void transformTextFile(final String filename) throws IOException {
		String content = FileAccess.loadTextFile(filename);
		String improvedContent = addImplicitListRules(content);
		FileAccess.saveTextFile(filename, improvedContent);
	}

	protected String addImplicitListRules(final String content) {
		// realizes String.replaceAll and prints out each replacement
		Matcher matcher = pattern.matcher(content);
		StringBuffer sb = new StringBuffer();

		while (matcher.find()) {
			matcher.appendReplacement(sb, REPLACEMENT);
			System.out.println(matcher.group(0));
		}
		matcher.appendTail(sb);

		return sb.toString();
	}

}
