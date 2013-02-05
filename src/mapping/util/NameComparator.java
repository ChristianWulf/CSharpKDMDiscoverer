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

import java.util.Comparator;

import org.eclipse.gmt.modisco.omg.kdm.core.KDMEntity;

public class NameComparator implements Comparator<KDMEntity> {

	@Override
	public int compare(final KDMEntity o1, final KDMEntity o2) {
		return o1.getName().compareToIgnoreCase(o2.getName());
	}

}
