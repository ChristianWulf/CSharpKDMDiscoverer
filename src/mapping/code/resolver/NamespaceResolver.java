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

package mapping.code.resolver;

import mapping.KDMElementFactory;
import mapping.code.extern.TypeNotFoundException;

import org.eclipse.gmt.modisco.omg.kdm.code.CodeItem;
import org.eclipse.gmt.modisco.omg.kdm.code.Module;
import org.eclipse.gmt.modisco.omg.kdm.code.Namespace;
import org.eclipse.gmt.modisco.omg.kdm.kdm.Attribute;

public class NamespaceResolver implements Resolver {

	@Override
	public CodeItem fs_findItem(final String identifier, final CodeItem parent)
			throws TypeNotFoundException {
		String prefix = KDMElementFactory.getQualifiedNameAttribute(parent).getValue();
		if (!prefix.isEmpty()) prefix += ".";

		Attribute qualifiedNameAttribute = KDMElementFactory
				.createFullyQualifiedNameAttribute(prefix + identifier);
		Namespace child = KDMElementFactory.createNamespaceUnit(qualifiedNameAttribute);
		child.setName(identifier);

		((Namespace) parent).getGroupedCode().add(child);
		((Module) parent.eContainer()).getCodeElement().add(child);
		// else if (child instanceof Datatype) {
		// ((CodeModel) searcher.externalGlobalNamespace.eContainer().eContainer())
		// .getCodeElement().add(child);
		// }
		return child;
	}

}
