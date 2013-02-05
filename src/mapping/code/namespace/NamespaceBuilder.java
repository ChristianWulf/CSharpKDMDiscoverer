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

import java.util.List;

import mapping.code.namespace.listener.BuildNamespaceHierarchyListener;
import mapping.code.namespace.listener.KdmListener;
import mapping.code.resolver.IdentifierResolver;

import org.eclipse.gmt.modisco.omg.kdm.code.Datatype;
import org.eclipse.gmt.modisco.omg.kdm.code.Namespace;

public class NamespaceBuilder {

	private final NamespaceSearcher			searcher;

	private final KdmListener<Namespace>	buildListener;

	public NamespaceBuilder(final NamespaceSearcher searcher,
			final IdentifierResolver identifierResolver) {
		this.searcher = searcher;
		this.buildListener = new BuildNamespaceHierarchyListener(identifierResolver);
	}

	/**
	 * Inserts the given <code>datatype</code> into the external code model. If the corresponding
	 * namespace hierarchy does not or only partly exist, it is created.
	 * 
	 * @param namespaceParts
	 * @param datatype
	 */
	@Deprecated
	public void insertDatatypeIntoExternalCodeModel(final List<String> namespaceParts,
			final Datatype datatype) {
		insertDatatypeIntoNamespaceHierarchy(namespaceParts, datatype,
				searcher.externalGlobalNamespace);
	}

	@Deprecated
	private void insertDatatypeIntoNamespaceHierarchy(final List<String> namespaceParts,
			final Datatype datatype, final Namespace parent) {
		Namespace namespace = updateNamespaceHierarchy(namespaceParts, parent);
		// add interface or class unit respectively
		namespace.getGroupedCode().add(datatype);
	}

	public Namespace updateNamespaceHierarchy(final List<String> namespaceParts,
			final Namespace parent) {
		Namespace namespace = searcher.findItem(namespaceParts, parent, buildListener, Namespace.class);
		return namespace;
	}

}
