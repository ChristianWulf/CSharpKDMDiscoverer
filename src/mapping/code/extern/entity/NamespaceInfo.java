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

import mapping.code.extern.reference.ReferenceInfo;

public class NamespaceInfo implements ReferenceInfo {

	private final String	name;
	private String	qualifiedPath;

	public NamespaceInfo(final String name, final String qualifiedPath) {
		this.name = name;
		this.qualifiedPath = qualifiedPath;
	}

	@Override
	public String getName() {
		return name;
	}

	public String getQualifiedPath() {
		return qualifiedPath;
	}

	public void setQualifiedPath(final String qualifiedPath) {
		this.qualifiedPath = qualifiedPath;
	}

	@Override
	public String toString() {
		return "NamespaceInfo [name=" + name + ", qualifiedPath="
				+ qualifiedPath + "]";
	}

	@Override
	public DatatypeInfo getOwner() {
		return null;
	}
}
