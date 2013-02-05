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

package mapping.code.extern.entity;

import java.util.Collection;
import java.util.HashMap;
import java.util.LinkedList;
import java.util.List;
import java.util.Map;

import mapping.code.extern.reference.ReferenceInfo;

public class DatatypeInfo implements ReferenceInfo {
	private String							name;
	private final String					type;
	private final List<String>				namespaceParts;
	private final Map<String, FieldInfo>	fields;
	private Boolean							isAbstract	= Boolean.FALSE;
	private final Map<String, MethodInfo>	methods;
	private final List<String>				fullPath	= new LinkedList<String>();

	public DatatypeInfo(final String type) {
		super();
		this.type = type;
		this.namespaceParts = new LinkedList<String>();
		this.fields = new HashMap<String, FieldInfo>();
		this.methods = new HashMap<String, MethodInfo>();
	}

	public void setName(final String name) {
		this.name = name;
	}

	@Override
	public String getName() {
		return name;
	}

	public Boolean isAbstract() {
		return isAbstract;
	}

	public void setIsAbstract(final Boolean b) {
		this.isAbstract = b;
	}

	@Override
	public String toString() {
		final int capacity = 17 + getName().length() + 25 + 5 + 25 + 5 + 30 + namespaceParts.size() * 10 + 10
				+ fields.size() * 30 + 11 + methods.size() * 30;
		StringBuilder stringBuilder = new StringBuilder(capacity);
		stringBuilder.append("name = ");
		stringBuilder.append(getName());
		stringBuilder.append("\ntype = ");
		stringBuilder.append(type);
		stringBuilder.append("\nisAbstract = ");
		stringBuilder.append(isAbstract);
		stringBuilder.append("\nnamespaceParts = ");
		stringBuilder.append(namespaceParts);
		stringBuilder.append("\nfields = ");
		stringBuilder.append(fields.values());
		stringBuilder.append("\nmethods = ");
		stringBuilder.append(methods.values());
		return stringBuilder.toString();
	}

	public void setNamespace(final Collection<String> namespaceParts) {
		this.namespaceParts.clear();
		this.namespaceParts.addAll(namespaceParts);
	}

	public List<String> getNamespace() {
		return namespaceParts;
	}

	public FieldInfo getFieldInfo(final String fieldName) {
		return fields.get(fieldName);
	}

	public void addFieldInfo(final FieldInfo fieldInfo) {
		this.fields.put(fieldInfo.getName(), fieldInfo);
	}

	public MethodInfo getMethodInfo(final String methodName) {
		return methods.get(methodName);
	}

	public void addMethodInfo(final MethodInfo methodInfo) {
		// TODO methodInfo.toString()
		this.methods.put(methodInfo.getName(), methodInfo);
	}

	@Override
	public DatatypeInfo getOwner() {
		return null;
	}

	public List<String> getFullPath() {
		return this.fullPath;
	}

	public String getType() {
		return type;
	}

	public boolean isInterface() {
		return type.equals("interface");
	}

}
