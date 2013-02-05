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
import java.util.ArrayList;
import java.util.LinkedList;
import java.util.List;
import java.util.regex.Matcher;
import java.util.regex.Pattern;

import util.FileAccess;

public class TreeClassGenerator {

	private static final String	COMPILATION_UNIT	= "compilation_unit";

	private static class TreeClass {

		String			className;
		List<String>	parameterNames	= new ArrayList<String>();

		public TreeClass() {
		}

		@Override
		public String toString() {
			return className + parameterNames;
		}

	}

	public TreeClassGenerator(final String filename) throws IOException {
		String content = FileAccess.loadTextFile(filename);
		String clearedContent = clearContent(content);
		List<TreeClass> treeClasses = parseTreeClasses(clearedContent);

		System.out.println(treeClasses);

		for (TreeClass treeClass : treeClasses) {
			String code = generateClassCode(treeClass);
			createJavaSourceCodeFile(treeClass.className, code);
		}
	}

	private String generateClassCode(final TreeClass treeClass) {
		StringBuilder sb = new StringBuilder();
		sb.append("package lang.csharp;\n");
		sb.append("\n");
		sb.append("import org.antlr.runtime.CommonToken;\n");
		sb.append("import org.antlr.runtime.tree.CommonTree;\n");
		sb.append("import org.antlr.runtime.tree.Tree;\n");

		sb.append("\n");
		sb.append("public class " + treeClass.className + " extends CommonTree {\n\n");
		for (String p : treeClass.parameterNames) {
			sb.append("\tprivate final Object " + p + ";\n");
		}
		sb.append("\n");
		sb.append("\tpublic " + treeClass.className + "(final int ttype");
		for (String p : treeClass.parameterNames) {
			sb.append(", final Object " + p);
		}
		sb.append(") {\n");
		for (String p : treeClass.parameterNames) {
			sb.append("\t\tthis." + p + " = " + p + ";\n");
		}
		sb.append("\t\tthis.token = new CommonToken(ttype);\n");
		for (String p : treeClass.parameterNames) {
			sb.append("\t\taddChild((Tree) " + p + ");\n");
		}
		sb.append("\t}\n\n");

		sb.append("\t@Override\n");
		sb.append("\tpublic String toString() {\n");
		sb.append("\t\treturn \"" + treeClass.className + ": \"");
		for (String p : treeClass.parameterNames) {
			sb.append(" + \"| " + p + "=\" + this." + p);
		}
		sb.append(";\n");
		sb.append("\t}\n");

		sb.append("}\n");
		return sb.toString();
	}

	private void createJavaSourceCodeFile(final String className, final String code)
			throws IOException {
		String filename = "src/lang/csharp/" + className + ".java";
		FileAccess.saveTextFile(filename, code);
	}

	private String clearContent(final String content) {
		int beginIndex = content.indexOf(COMPILATION_UNIT);
		String clearedContent = content.substring(beginIndex);

		// clear line comments (.* stops at newline)
		clearedContent = clearedContent.replaceAll("//.*", "");
		// clear white spaces incl. newline
		clearedContent = clearedContent.replaceAll("\\s{2,}", "");

		return clearedContent;
	}

	private List<TreeClass> parseTreeClasses(final String content) {
		List<TreeClass> treeClasses = new LinkedList<TreeClass>();

		Matcher matcher = Pattern.compile("\\w\\<.*?\\>").matcher(content);
		while (matcher.find()) {
			TreeClass treeClass = new TreeClass();

			final int endIndex = matcher.end();
			treeClass.className = content.substring(matcher.start() + 2, endIndex - 1);

			int paramEndIndex = content.indexOf("]", endIndex);
			String parameterString = content.substring(endIndex, paramEndIndex);
			String[] parameters = parameterString.split(",");
			Pattern paramPattern = Pattern.compile("\\$.*?\\.tree");
			for (String p : parameters) {
				System.out.print("Param(old): " + p);
				Matcher paramMatcher = paramPattern.matcher(p);
				paramMatcher.find();
				// TODO walk back to get the long parameter name
				String param = p.substring(paramMatcher.start() + 1, paramMatcher.end() - 5);
				System.out.println(" -> " + param);
				treeClass.parameterNames.add(param);
			}

			treeClasses.add(treeClass);
		}

		return treeClasses;
	}

	public static void main(final String[] args) throws IOException {
		TreeClassGenerator treeClassGenerator = new TreeClassGenerator("grammars/CsRewriteRules.g");
	}
}
