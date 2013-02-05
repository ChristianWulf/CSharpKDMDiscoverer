/***************************************************************************
 * Copyright 2013 by
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

package de.cau.chw.transformation.external;

import java.util.Collections;
import java.util.List;

/**
 * @author chw
 */
public class ExternalEntity {

	public final List<String>	qualifiedEntityName;
	public final List<Integer>	entityExtensions;

	public ExternalEntity(final List<String> qualifiedEntityName, final List<Integer> entityExtensions) {
		this.qualifiedEntityName = qualifiedEntityName;
		this.entityExtensions = entityExtensions;
	}

	/**
	 * For a namespace or non-array, non-variable, non-pointer type
	 * 
	 * @param qualifiedEntityName
	 */
	public ExternalEntity(final List<String> qualifiedEntityName) {
		this(qualifiedEntityName, Collections.<Integer> emptyList());
	}

	@Override
	public int hashCode() {
		final int prime = 31;
		int result = 1;
		result = prime * result + ((entityExtensions == null) ? 0 : entityExtensions.hashCode());
		result = prime * result + ((qualifiedEntityName == null) ? 0 : qualifiedEntityName.hashCode());
		return result;
	}

	@Override
	public boolean equals(final Object obj) {
		if (this == obj) return true;
		if (obj == null) return false;
		if (!(obj instanceof ExternalEntity)) return false;
		ExternalEntity other = (ExternalEntity) obj;
		if (entityExtensions == null) {
			if (other.entityExtensions != null) return false;
		} else if (!entityExtensions.equals(other.entityExtensions)) return false;
		if (qualifiedEntityName == null) {
			if (other.qualifiedEntityName != null) return false;
		} else if (!qualifiedEntityName.equals(other.qualifiedEntityName)) return false;
		return true;
	}

	@Override
	public String toString() {
		return "ExternalEntity [qualifiedEntityName=" + qualifiedEntityName + ", entityExtensions=" + entityExtensions
				+ "]";
	}

}
