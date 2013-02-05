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

public class FieldInfo implements ReferenceInfo {

	/**
	 * The attribute type is only the type name at first. If it is
	 * accessed/needed, the type is loaded from the file system.
	 */
	public interface AttributeType<T> {
		T getType();
	}

	public class NoType implements AttributeType<String> {

		private final String typeName;

		public NoType(final String typeName) {
			this.typeName = typeName;
		}

		@Override
		public String getType() {
			return typeName;
		}

	}

	private String name;
	private AttributeType<?> type;
	private final DatatypeInfo owner;

	public FieldInfo(final DatatypeInfo owner) {
		this.owner = owner;
	}

	@Override
	public String getName() {
		return this.name;
	}

	public void setName(final String name) {
		this.name = name;
	}

	public AttributeType<?> getType() {
		return type;
	}

	public void setType(final AttributeType<?> type) {
		this.type = type;
	}

	public void setType(final String typeName) {
		this.type = new NoType(typeName);
	}

	@Override
	public int hashCode() {
		final int prime = 31;
		int result = 1;
		result = prime * result + ((name == null) ? 0 : name.hashCode());
		result = prime * result + ((owner == null) ? 0 : owner.hashCode());
		result = prime * result + ((type == null) ? 0 : type.hashCode());
		return result;
	}

	@Override
	public boolean equals(final Object obj) {
		if (this == obj)
			return true;
		if (obj == null)
			return false;
		if (!(obj instanceof FieldInfo))
			return false;
		FieldInfo other = (FieldInfo) obj;
		if (name == null) {
			if (other.name != null)
				return false;
		} else if (!name.equals(other.name))
			return false;
		if (owner == null) {
			if (other.owner != null)
				return false;
		} else if (!owner.equals(other.owner))
			return false;
		if (type == null) {
			if (other.type != null)
				return false;
		} else if (!type.equals(other.type))
			return false;
		return true;
	}

	@Override
	public DatatypeInfo getOwner() {
		return owner;
	}

}
