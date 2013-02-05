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

import java.util.Collection;
import java.util.LinkedList;
import java.util.List;

import mapping.KDMElementFactory;
import mapping.code.namespace.listener.KdmListener;
import mapping.code.namespace.listener.NullListener;
import mapping.util.KDMChildHelper;

import org.eclipse.core.runtime.Assert;
import org.eclipse.emf.common.util.EList;
import org.eclipse.gmt.modisco.omg.kdm.code.AbstractCodeElement;
import org.eclipse.gmt.modisco.omg.kdm.code.AbstractCodeRelationship;
import org.eclipse.gmt.modisco.omg.kdm.code.CodeItem;
import org.eclipse.gmt.modisco.omg.kdm.code.CodeModel;
import org.eclipse.gmt.modisco.omg.kdm.code.CompilationUnit;
import org.eclipse.gmt.modisco.omg.kdm.code.Datatype;
import org.eclipse.gmt.modisco.omg.kdm.code.Extends;
import org.eclipse.gmt.modisco.omg.kdm.code.Imports;
import org.eclipse.gmt.modisco.omg.kdm.code.Module;
import org.eclipse.gmt.modisco.omg.kdm.code.Namespace;

import util.Logger;

public class NamespaceSearcher {

	private static final Logger LOGGER = new Logger(NamespaceSearcher.class);

	public static final KdmListener<CodeItem> nullListener = new NullListener<CodeItem>();
	public static final KdmListener<Namespace> nsNullListener = new NullListener<Namespace>();

	public Namespace internalGlobalNamespace;
	public Namespace externalGlobalNamespace;

	public NamespaceSearcher(final Namespace internalGlobalNamespace,
			final Namespace externalGlobalNamespace) {
		this();
		// Assert.isNotNull(internalGlobalNamespace);
		Assert.isNotNull(externalGlobalNamespace);
		this.internalGlobalNamespace = internalGlobalNamespace;
		this.externalGlobalNamespace = externalGlobalNamespace;
	}

	public NamespaceSearcher() {
		super();
	}

	/**
	 * Each {@link CodeModel} must have a {@link Module} that owns one single
	 * element, namely the global {@link Namespace}.
	 * 
	 * @param codeModel
	 * @return the global namespace of this codeModel
	 */
	public static Namespace getGlobalNamespaceFrom(final CodeModel codeModel) {
		for (AbstractCodeElement ce : codeModel.getCodeElement()) {
			if (ce instanceof Module
					&& KDMElementFactory.NAMESPACES_MODULE.equals(ce.getName())) {
				Module namespaceModule = (Module) ce;
				if (namespaceModule.getCodeElement().size() > 0) {
					AbstractCodeElement globalNamespace = namespaceModule
							.getCodeElement().get(0);
					if (globalNamespace instanceof Namespace
							&& KDMElementFactory.GLOBAL_NAMESPACE_NAME
							.equals(globalNamespace.getName())) {
						return (Namespace) globalNamespace;
					}
				}
			}
		}
		throw new IllegalStateException(
				"Each CodeModel must have a module containing the global namespace. "
						+ "This CodeModel does not have anyone.");
	}

	/**
	 * @param qualifiedNamespace
	 * @param listener
	 * @return null if the namespace is neither in the internal nor in the
	 *         external namespace hierarchy so far
	 */
	public Namespace findNamespaceFromGlobals(final List<String> qualifiedNamespace,
			final KdmListener<Namespace> listener) {
		Namespace namespace;
		// 1. search fully qualified namespace in internal namespaces
		namespace = findItem(qualifiedNamespace, internalGlobalNamespace, listener,
				Namespace.class);
		// 2. search fully qualified namespace in external namespaces
		if (namespace == null) {
			namespace = findItem(qualifiedNamespace, externalGlobalNamespace, listener,
					Namespace.class);
		}
		return namespace;
	}

	<T extends CodeItem> T findItem(final Collection<String> qualifiedNamespace,
			final T startParent, final KdmListener<T> listener, final Class<T> clazz) {
		T parent = startParent;
		for (String identPart : qualifiedNamespace) {
			T item = KDMChildHelper.getChildItemByName(identPart, parent, clazz);
			if (item == null) {
				item = listener.onNullItem(identPart, parent);
				if (item == null)
					return null;
			}
			parent = item;
			listener.onIterationEnd(parent);
		}

		return parent;
	}

	/**
	 * @param qualifiedNamespace
	 * @param parent
	 * @param usingDirectives
	 *            w/o global namespace
	 * @return
	 */
	public Namespace findNamespaceTryingAllImports(
			final Collection<String> qualifiedNamespace, final Namespace parent,
			final Collection<List<String>> usingDirectives) {
		if (!qualifiedNamespace.isEmpty()) {
			// w/o prefix, i.e. do not consider any using directive first
			Namespace namespace = findItem(qualifiedNamespace, parent, nsNullListener,
					Namespace.class);
			if (namespace != null)
				return namespace;
		}

		List<String> namespaceParts = new LinkedList<String>();
		for (Collection<String> prefix : usingDirectives) {
			namespaceParts.addAll(prefix);
			namespaceParts.addAll(qualifiedNamespace);

			Namespace namespace = findItem(namespaceParts, parent, nsNullListener,
					Namespace.class);
			if (namespace != null)
				return namespace;

			namespaceParts.clear();
		}
		return null;
	}

	public List<Imports> getImportsFromNamespaceHierarchy(
			final CompilationUnit compilationUnit) {
		List<Imports> imports = new LinkedList<Imports>();

		EList<AbstractCodeRelationship> codeRelations = compilationUnit.getCodeRelation();
		for (int i = 0; i < codeRelations.size(); i++) {
			AbstractCodeRelationship r = codeRelations.get(i);
			if (r instanceof Imports) {
				imports.add((Imports) r);
			}
		}

		return imports;
	}

	@Deprecated
	public CodeItem findItemTryingAllImports(final String identifier,
			final Namespace parent, final Collection<List<String>> usingDirectives) {
		// System.out.println("NamespaceSearcher.findItemTryingAllImports("+identifier+","+parent+")");
		// w/o prefix, i.e. do not consider any using directive first
		CodeItem item = KDMChildHelper.getChildItemByName(identifier, parent,
				CodeItem.class);
		if (item != null)
			return item;

		List<String> itemPath = new LinkedList<String>();
		for (Collection<String> prefix : usingDirectives) {
			itemPath.addAll(prefix);
			itemPath.add(identifier);

			item = findItem(itemPath, parent, nullListener, CodeItem.class);
			if (item != null)
				return item;

			itemPath.clear();
		}
		return null;
	}

	public CodeItem getItemByNameFromInheritance(final String identifier,
			final Datatype parent, final Class<? extends CodeItem> clazz) {
		Extends extendz = null;
		for (AbstractCodeRelationship rel : parent.getCodeRelation()) {
			// System.out.println("extends/implements: "+rel);
			if (rel instanceof Extends) {
				extendz = (Extends) rel;
				break;
			}
		}
		// if the parent does not extends from a super type
		if (extendz == null)
			return null;
		// unsupported case: NamedEdge<R> : NamedEdge<S,T>
		if (parent == extendz.getTo()) {
			return null;
		}
		// search child type within super type
		CodeItem item;
		if (identifier.contains("(")) {
			item = KDMChildHelper.getChildMethodByName(identifier, extendz.getTo());
		} else {
			item = KDMChildHelper.getChildItemByName(identifier, extendz.getTo(), clazz);
		}
		if (item != null)
			return item;
		// if not found, search recursively in the super type of the super type
		return getItemByNameFromInheritance(identifier, extendz.getTo(), clazz);
	}

	/**
	 * @param identifier
	 *            of a namespace or a type
	 * @param usingDirectives
	 * @return
	 */
	public CodeItem findItemTryingAllImports(final String identifier,
			final List<CodeItem> usingDirectives) {
		LOGGER.fine("trying " + usingDirectives.size() + " using directives...");
		for (CodeItem using : usingDirectives) {
			// System.out.println("NamespaceSearcher.findItemTryingAllImports() -> try using: "
			// + using.getName());
			CodeItem item = KDMChildHelper.getChildItemByName(identifier, using,
					CodeItem.class);
			if (item != null) {
				return item;
			}
		}
		return null;
	}

}