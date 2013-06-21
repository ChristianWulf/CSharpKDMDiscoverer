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

import java.util.Collection;
import java.util.Collections;
import java.util.Deque;
import java.util.LinkedList;
import java.util.List;
import java.util.ListIterator;
import java.util.Map;

import mapping.KDMElementFactory;
import mapping.code.MoDiscoKDM;
import mapping.code.extern.ExternalDatatypeInfoRepository;
import mapping.code.extern.TypeNotFoundException;
import mapping.code.namespace.NamespaceSearcher;
import mapping.code.namespace.NamespaceStack;
import mapping.code.transformator.ExpressionNotSupported;
import mapping.util.EMFHelper;
import mapping.util.KDMChildHelper;

import org.eclipse.core.runtime.Assert;
import org.eclipse.emf.ecore.EObject;
import org.eclipse.gmt.modisco.omg.kdm.action.ActionElement;
import org.eclipse.gmt.modisco.omg.kdm.code.AbstractCodeElement;
import org.eclipse.gmt.modisco.omg.kdm.code.ArrayType;
import org.eclipse.gmt.modisco.omg.kdm.code.ClassUnit;
import org.eclipse.gmt.modisco.omg.kdm.code.CodeItem;
import org.eclipse.gmt.modisco.omg.kdm.code.CodeModel;
import org.eclipse.gmt.modisco.omg.kdm.code.CompilationUnit;
import org.eclipse.gmt.modisco.omg.kdm.code.CompositeType;
import org.eclipse.gmt.modisco.omg.kdm.code.DataElement;
import org.eclipse.gmt.modisco.omg.kdm.code.Datatype;
import org.eclipse.gmt.modisco.omg.kdm.code.Imports;
import org.eclipse.gmt.modisco.omg.kdm.code.InterfaceUnit;
import org.eclipse.gmt.modisco.omg.kdm.code.MemberUnit;
import org.eclipse.gmt.modisco.omg.kdm.code.MethodUnit;
import org.eclipse.gmt.modisco.omg.kdm.code.Namespace;
import org.eclipse.gmt.modisco.omg.kdm.code.ParameterUnit;
import org.eclipse.gmt.modisco.omg.kdm.code.StorableUnit;
import org.eclipse.gmt.modisco.omg.kdm.kdm.Attribute;

import util.ListUtil;
import util.Logger;

public class IdentifierResolver extends FSResolver {

	private static final Logger LOGGER = new Logger(IdentifierResolver.class);

	private final Map<String, Imports> aliases;

	private final CodeModel externalCodeModel;

	public IdentifierResolver(final NamespaceSearcher searcher,
			final ExternalDatatypeInfoRepository datatypeInfoRepository,
			final Map<String, Imports> aliases, final CodeModel externalCodeModel) {
		super(datatypeInfoRepository, searcher);
		this.aliases = aliases;
		this.externalCodeModel = externalCodeModel;
	}

	// also resolves cl.temp, i.e. member access: ParameterUnit.StorableUnit
	public CodeItem resolveType(final List<String> typeName,
			final List<CodeItem> declarations,
			final NamespaceStack internalNamespaceStack,
			final CompilationUnit compilationUnit, final CodeItem parent)
					throws TypeNotFoundException {
		// System.out.println("IdentifierResolver.resolveType: "
		// + typeName.toString());
		String identifier = typeName.get(0);

		CodeItem type = resolveIdentifier(identifier, declarations,
				internalNamespaceStack, compilationUnit, parent);
		// e.g. because type is a variable of a type parameter
		if (type == null)
			throw new TypeNotFoundException(typeName.toString(), null);

		for (int i = 1; i < typeName.size(); i++) {
			identifier = typeName.get(i);
			type = resolveFurtherIdentifier(type, identifier);
		}

		// e.g. because the required using directive was not declared
		if (type == null)
			throw new TypeNotFoundException(typeName.toString(), null);
		return type;
	}

	@Deprecated
	private Collection<List<String>> getUsingDirectives1(
			final CompilationUnit compilationUnit,
			final NamespaceStack internalNamespaceStack) {
		List<Imports> imports = searcher
				.getImportsFromNamespaceHierarchy(compilationUnit);
		Collection<Namespace> namespacePath = internalNamespaceStack
				.getCurrentNamespacePath();

		// TODO return a List of CodeItems (Namespace &
		// ClassUnit/InterfaceUnit..)

		Collection<List<String>> usingDirectives = new LinkedList<List<String>>();

		usingDirectives.add(Collections.<String> emptyList()); // global
		// namespace
		for (Imports imp : imports) {
			if (namespacePath.contains(imp.getFrom())) {
				Namespace usingNamespace = (Namespace) imp.getTo();
				Attribute attr = KDMElementFactory
						.getQualifiedNameAttribute(usingNamespace);
				List<String> using = ListUtil.splitAtDot(attr.getValue());
				usingDirectives.add(using);
			}
		}
		for (Namespace n : namespacePath) {
			Attribute attr = KDMElementFactory.getQualifiedNameAttribute(n);
			List<String> using = ListUtil.splitAtDot(attr.getValue());
			usingDirectives.add(using);
		}
		return usingDirectives;
	}

	private List<CodeItem> getUsingDirectives(final CompilationUnit compilationUnit,
			final NamespaceStack internalNamespaceStack) {
		List<Imports> imports = searcher
				.getImportsFromNamespaceHierarchy(compilationUnit);
		List<Namespace> namespacePath = internalNamespaceStack.getCurrentNamespacePath();

		List<CodeItem> usingDirectives = new LinkedList<CodeItem>();

		for (Imports imp : imports) {
			if (namespacePath.contains(imp.getFrom())) {
				usingDirectives.add(imp.getTo());
			}
		}
		usingDirectives.addAll(namespacePath);
		usingDirectives.add(searcher.externalGlobalNamespace); // at last; order
		// is important
		return usingDirectives;
	}

	/**
	 * @param identifier
	 * @param declarations
	 * @param internalNamespaceStack
	 * @param compilationUnit
	 * @param externalCodeModel
	 * @return e.g. ClassUnit, MethodUnit, StorableUnit
	 */
	public CodeItem resolveIdentifier(final String identifier,
			final List<CodeItem> declarations,
			final NamespaceStack internalNamespaceStack,
			final CompilationUnit compilationUnit, final CodeItem parent) {
		LOGGER.fine("IdentifierResolver.resolveIdentifier().identifier = " + identifier);
		LOGGER.fine("IdentifierResolver.resolveIdentifier().parent = " + parent);

		// 1. search in local variables and in class member variables backwards
		// to respect scope
		// precedence
		LOGGER.fine("Search in declarations...");
		for (ListIterator<CodeItem> iter = declarations.listIterator(); iter.hasNext();) {
			CodeItem element = iter.next();
			// System.out.println("Check declaration: " + element);
			if (identifier.equals(element.getName())) {
				return element;
			}
		}

		CodeItem codeItem;

		// 1b. search in parent
		LOGGER.fine("Search in children...");
		if (identifier.contains("(")) {
			codeItem = KDMChildHelper.getChildMethodByName(identifier, parent);
			if (codeItem != null)
				return codeItem;
		}

		// 2. search in aliases
		LOGGER.fine("Search in aliases...");
		codeItem = findAlias(identifier);
		if (codeItem != null)
			return codeItem;

		// get all using directives in scope
		Collection<List<String>> usingDirectives1 = getUsingDirectives1(compilationUnit,
				internalNamespaceStack);
		List<CodeItem> usingDirectives = getUsingDirectives(compilationUnit,
				internalNamespaceStack);
		List<CodeItem> externalUsingDirectives = extractExternalUsingDirectives(usingDirectives);

		// 3. search in internal namespace
		LOGGER.fine("Search in internal namespace...");
		codeItem = searcher.findItemTryingAllImports(identifier, usingDirectives);
		if (codeItem != null)
			return codeItem;

		// 4. search in external namespace
		LOGGER.fine("Search in external namespace...");
		codeItem = searcher.findItemTryingAllImports(identifier,
				searcher.externalGlobalNamespace, usingDirectives1);
		if (codeItem != null)
			return codeItem;

		// 5. search in type's inheritance relationships
		Datatype parentType = EMFHelper.goUpToParentType(parent, Datatype.class);
		LOGGER.fine("Search in super types...(" + parentType + ")");
		if (parentType != null) {
			codeItem = searcher.getItemByNameFromInheritance(identifier, parentType,
					CodeItem.class);
			if (codeItem != null)
				return codeItem;
		}

		// 6. search on file system
		LOGGER.fine("Search on file system...");
		codeItem = fs_findItemTryingAllImports(identifier, externalUsingDirectives);
		if (codeItem != null)
			return codeItem;

		LOGGER.fine("later defined: " + identifier);
		return null;
	}

	private List<CodeItem> extractExternalUsingDirectives(
			final List<CodeItem> usingDirectives) {
		List<CodeItem> externalUsingDirectives = new LinkedList<CodeItem>();
		for (CodeItem ci : usingDirectives) {
			CodeModel parentCodeModel = EMFHelper.goUpToParentType(ci, CodeModel.class);
			if (parentCodeModel == externalCodeModel) {
				externalUsingDirectives.add(ci);
			}
		}
		return externalUsingDirectives;
	}

	private CodeItem findAlias(final String identifier) {
		Imports alias = aliases.get(identifier);
		if (alias == null)
			return null;
		// FIXME check whether the alias is in scope
		return alias.getTo();
	}

	public CodeItem resolveFurtherIdentifier(final CodeItem prefixReference,
			final String identifier) throws TypeNotFoundException {
		LOGGER.fine("[resolveFurtherIdentifier] prefixReference: " + prefixReference);
		//		System.out.println("[resolveFurtherIdentifier] prefixReference: " + prefixReference);
		LOGGER.fine("[resolveFurtherIdentifier] identifier: " + identifier);
		//		System.out.println("[resolveFurtherIdentifier] identifier: " + identifier);
		Assert.isNotNull(prefixReference);

		CodeItem parent = prefixReference;
		if (prefixReference instanceof DataElement) {
			parent = ((DataElement) prefixReference).getType();
			// support array.Length(), ColumnsCount() and other indirectly
			// defined methods on arrays
			if (parent instanceof ArrayType) {
				return prefixReference;
			}
		}
		LOGGER.fine("[resolveFurtherIdentifier] parent: " + parent);
		if (parent == null) {
			throw new ExpressionNotSupported(
					"[resolveFurtherIdentifier] parent is an unsupported type parameter");
		}

		// 1. search below prefixReference
		CodeItem codeItem;
		LOGGER.fine("[resolveFurtherIdentifier] Search in children...");
		if (identifier.contains("(")) {
			codeItem = KDMChildHelper.getChildMethodByName(identifier, parent);
			if (codeItem != null)
				return codeItem;
		} else {
			//			System.out.println("parent.children: "
			//					+ KDMChildHelper.getChildrenFromCodeItem(parent));
			codeItem = KDMChildHelper.getChildItemByName(identifier, parent,
					CodeItem.class);
			if (codeItem != null)
				return codeItem;
		}

		// 2. search in type's inheritance relationships
		Datatype parentType = EMFHelper.goUpToParentType(parent, Datatype.class);
		LOGGER.fine("[resolveFurtherIdentifier] Search in super types of '" + parentType
				+ "'...");
		if (parentType != null) {
			codeItem = searcher.getItemByNameFromInheritance(identifier, parentType,
					CodeItem.class);
			if (codeItem != null)
				return codeItem;
		}

		LOGGER.fine("[resolveFurtherIdentifier] Search on file system...");
		// 3. search on file system
		codeItem = fs_findItem(identifier, parent);
		if (codeItem != null)
			return codeItem;

		throw new TypeNotFoundException(identifier, null);
	}

	public List<CodeItem> resolveDeclarations(final Deque<List<CodeItem>> declarations) {
		return ListUtil.flatten(declarations);
	}

	@Deprecated
	public List<CodeItem> resolveDeclarations(final EObject bottom) {
		List<CodeItem> declarations = new LinkedList<CodeItem>();

		EObject parent = bottom;
		while (!(parent instanceof CompilationUnit)) {
			List<? extends AbstractCodeElement> items = null;
			if (parent instanceof Namespace) {
				items = ((Namespace) parent).getGroupedCode();
			} else if (parent instanceof ClassUnit) {
				items = ((ClassUnit) parent).getCodeElement();
			} else if (parent instanceof InterfaceUnit) {
				items = ((InterfaceUnit) parent).getCodeElement();
			} else if (parent instanceof CompositeType) {
				items = ((CompositeType) parent).getItemUnit();
			} else if (parent instanceof MethodUnit) {
				items = ((MethodUnit) parent).getCodeElement();
			}

			if (items == null)
				throw new IllegalStateException(
						"UNSUPPORTED: Cannot resolve declarations of type '"
								+ parent.getClass() + "'");

			for (AbstractCodeElement c : items) {
				if (c instanceof StorableUnit || c instanceof ParameterUnit
						|| c instanceof MethodUnit || c instanceof MemberUnit) {
					declarations.add((CodeItem) c);
				} else if (c instanceof ActionElement) { // MoDisco format
					ActionElement ac = (ActionElement) c;
					if (ac.getKind().equals(MoDiscoKDM.VARIABLE_ACCESS)) {
						StorableUnit storableUnit = (StorableUnit) ac.getCodeElement()
								.get(0);
						declarations.add(storableUnit);
					}
				}
			}

			parent = parent.eContainer();
		}

		return declarations;
	}

}
