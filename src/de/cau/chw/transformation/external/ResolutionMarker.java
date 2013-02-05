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

import org.eclipse.gmt.modisco.omg.kdm.core.KDMEntity;
import org.eclipse.gmt.modisco.omg.kdm.core.ModelElement;

/**
 * @author chw
 */
public class ResolutionMarker {

	public final Class<? extends ModelElement>	externalEntityClass;
	public final KDMEntity						kdmEntity;

	public ResolutionMarker(final Class<? extends ModelElement> externalEntityClass, final KDMEntity kdmEntity) {
		this.externalEntityClass = externalEntityClass;
		this.kdmEntity = kdmEntity;
	}

	@Override
	public String toString() {
		return "ResolutionMarker [externalEntityClass=" + externalEntityClass + ", kdmEntity=" + kdmEntity + "]";
	}

	@Override
	public int hashCode() {
		final int prime = 31;
		int result = 1;
		result = prime * result + ((externalEntityClass == null) ? 0 : externalEntityClass.hashCode());
		result = prime * result + ((kdmEntity == null) ? 0 : kdmEntity.hashCode());
		return result;
	}

	@Override
	public boolean equals(final Object obj) {
		if (this == obj) return true;
		if (obj == null) return false;
		if (!(obj instanceof ResolutionMarker)) return false;
		ResolutionMarker other = (ResolutionMarker) obj;
		if (externalEntityClass == null) {
			if (other.externalEntityClass != null) return false;
		} else if (!externalEntityClass.equals(other.externalEntityClass)) return false;
		if (kdmEntity == null) {
			if (other.kdmEntity != null) return false;
		} else if (!kdmEntity.equals(other.kdmEntity)) return false;
		return true;
	}

}
