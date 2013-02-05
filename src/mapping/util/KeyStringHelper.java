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

package mapping.util;

import java.util.List;

import org.eclipse.gmt.modisco.omg.kdm.code.ParameterKind;
import org.eclipse.gmt.modisco.omg.kdm.code.ParameterUnit;

public class KeyStringHelper {

	private KeyStringHelper() {
		// utility class
	}

	public static String normalize(final String text) {
		return text.replaceFirst("\\bString\\b", "string");
	}

	public static String getMethodKey(final String qualifiedMethodName,
			final List<String> formalParameters) {
		StringBuilder builder = new StringBuilder();
		builder.append(qualifiedMethodName);
		builder.append("(");
		if (formalParameters != null) {
			for (int i = 0; i < formalParameters.size(); i++) {
				String paramTypeName = formalParameters.get(i);
				paramTypeName = normalize(paramTypeName);
				builder.append(paramTypeName);
				if (i < formalParameters.size() - 1)
					builder.append(",");
			}
		}
		builder.append(")");
		return builder.toString();
	}

	public static String buildSignatureKey(final String methodName,
			final List<ParameterUnit> parameters) {
		StringBuilder builder = new StringBuilder();
		builder.append(methodName);
		builder.append("(");
		//		System.out.println("buildSignatureKey() -> " + "methodName: "
		//				+ methodName + ", parameters: " + parameters);
		for (int i = 0; i < parameters.size(); i++) {
			ParameterUnit p = parameters.get(i);
			if (p.getKind() == ParameterKind.RETURN)
				continue;
			//			String qualifiedParamTypeName = KDMElementFactory
			//					.getQualifiedNameAttribute(p.getType()).getValue();
			String qualifiedParamTypeName = p.getType().getName();
			qualifiedParamTypeName = normalize(qualifiedParamTypeName);
			builder.append(qualifiedParamTypeName);
			if (i < parameters.size() - 1)
				builder.append(",");
		}
		builder.append(")");
		return builder.toString();
	}

}
