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

package mapping.code.extern.entity;

import java.util.LinkedList;
import java.util.List;

import mapping.code.extern.reference.ReferenceInfo;

public class MethodInfo implements ReferenceInfo {

	public static class ParameterInfo {
		public final String type;
		public final String value;

		public ParameterInfo(final String type, final String value) {
			this.type = type;
			this.value = value;
		}

		@Override
		public String toString() { // toString is used for building unique
			// identifier; must only return type
			return type;
		}
	}

	private String name;
	private final List<ParameterInfo> parameters = new LinkedList<ParameterInfo>();
	private String returnType;
	private final DatatypeInfo owner;

	public MethodInfo(final DatatypeInfo owner) {
		this.owner = owner;
	}

	@Override
	public String getName() {
		return name;
	}

	public void setName(final String name) {
		this.name = name;
	}

	public String getReturnType() {
		return returnType;
	}

	public void setReturnType(final String returnType) {
		this.returnType = returnType;
	}

	public List<ParameterInfo> getParameters() {
		return parameters;
	}

	@Override
	public String toString() {
		StringBuilder builder = new StringBuilder(name != null ? name.length()
				: 0 + parameters.size() * 10 + returnType.length() + 2 * 3);
		builder.append(name);
		builder.append("(");
		builder.append(parameters);
		builder.append("):");
		builder.append(returnType);
		return builder.toString();
	}

	@Override
	public DatatypeInfo getOwner() {
		return owner;
	}

}
