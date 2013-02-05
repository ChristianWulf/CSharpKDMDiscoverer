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

package mapping.code.namespace.listener;

import mapping.code.resolver.Resolver;

import org.eclipse.gmt.modisco.omg.kdm.code.CodeItem;
import org.eclipse.gmt.modisco.omg.kdm.code.Namespace;

public class BuildNamespaceHierarchyListener implements KdmListener<Namespace> {

	private final Resolver	resolver;

	public BuildNamespaceHierarchyListener(final Resolver resolver) {
		this.resolver = resolver;
	}

	@Override
	public Namespace onNullItem(final String nsPart, final Namespace parent) {
		// ensure that namespace "nsPart" exists on file system
		CodeItem item = resolver.fs_findItem(nsPart, parent);
		return (Namespace) item;
	}

	@Override
	public void onIterationEnd(final Namespace namespace) {
		// do nothing
	}

}
