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

package mapping.code.namespace;

import java.util.HashSet;
import java.util.Iterator;
import java.util.Set;

import org.eclipse.emf.common.util.EList;
import org.eclipse.gmt.modisco.omg.kdm.code.CodeItem;
import org.eclipse.gmt.modisco.omg.kdm.code.Namespace;

public class NamespaceMerger {

	public void mergeNamespaces(final Namespace base, final Namespace ns) {
		// contains() has O(1) in HashSet
		Set<CodeItem> baseSet = new HashSet<CodeItem>(base.getGroupedCode());
		EList<CodeItem> nsElements = ns.getGroupedCode();

		//		new LinkedList<CodeItem>();

		for (Iterator<CodeItem> iter = nsElements.iterator(); iter.hasNext();) {
			CodeItem codeItem = iter.next();
			// TODO codeItem must implement equals
			// TODO propagate Map.getEntry:CodeItem instead of contains:boolean
			if (baseSet.contains(codeItem)) {
				// move ref if both contain the element
				// TODO
			} else {
				// move CodeItem (Namespace, CLassUnit) if base does not contain the element
				baseSet.add(codeItem);
				iter.remove();
			}
		}

	}
}
