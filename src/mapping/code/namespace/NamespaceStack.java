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

import java.util.LinkedList;
import java.util.List;

import mapping.code.namespace.listener.KdmListener;
import mapping.code.namespace.listener.StackListener;
import mapping.code.resolver.Resolver;

import org.eclipse.gmt.modisco.omg.kdm.code.Module;
import org.eclipse.gmt.modisco.omg.kdm.code.Namespace;

public class NamespaceStack implements Cloneable {

	private LinkedList<Namespace>			namespaceStack	= new LinkedList<Namespace>();
	private final NamespaceSearcher			searcher;
	private final KdmListener<Namespace>	stackListener;

	public NamespaceStack(final Namespace globalNamespace, final NamespaceSearcher searcher,
			final Resolver resolver) {
		this.searcher = searcher;
		this.namespaceStack.add(globalNamespace);
		this.stackListener = new StackListener(namespaceStack,
				(Module) globalNamespace.eContainer(), resolver);
	}

	public Namespace updateFromCurrentNamespace(final List<String> identifiers) {
		Namespace namespace = searcher.findItem(identifiers, getCurrentNamespace(), stackListener,
				Namespace.class);
		return namespace;
	}

	/**
	 * Only for test purposes
	 * 
	 * @param namespace
	 */
	void pushNamespace(final Namespace namespace) {
		namespaceStack.add(namespace);
	}

	public Namespace popCurrentNamespace(int count) {
		Namespace namespace;
		do {
			namespace = namespaceStack.removeLast();
		} while (--count > 0);
		return namespace;
	}

	public Namespace getCurrentNamespace() {
		return namespaceStack.getLast();
	}

	public Namespace goToGlobalNamespace() {
		return popCurrentNamespace(namespaceStack.size() - 1);
	}

	public List<Namespace> getCurrentNamespacePath() {
		return namespaceStack;
	}

	public void setCurrentNamespaceStack(final LinkedList<Namespace> namespaceStack) {
		this.namespaceStack = namespaceStack;
	}

}
