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

import java.io.FileNotFoundException;
import java.util.List;

import mapping.code.extern.ExternalDatatypeInfoRepository;
import mapping.code.extern.TypeNotFoundException;
import mapping.code.extern.loader.DatatypeInfoLoadException;
import mapping.code.extern.reference.ReferenceInfo;
import mapping.code.extern.reference.ReferenceInfo2CodeItemMapper;
import mapping.code.namespace.NamespaceSearcher;

import org.eclipse.gmt.modisco.omg.kdm.code.ClassUnit;
import org.eclipse.gmt.modisco.omg.kdm.code.CodeItem;
import org.eclipse.gmt.modisco.omg.kdm.code.CodeModel;
import org.eclipse.gmt.modisco.omg.kdm.code.CompositeType;
import org.eclipse.gmt.modisco.omg.kdm.code.Datatype;
import org.eclipse.gmt.modisco.omg.kdm.code.InterfaceUnit;
import org.eclipse.gmt.modisco.omg.kdm.code.ItemUnit;
import org.eclipse.gmt.modisco.omg.kdm.code.Module;
import org.eclipse.gmt.modisco.omg.kdm.code.Namespace;

import util.Pause;

public class FSResolver implements Resolver {

	private final ExternalDatatypeInfoRepository datatypeInfoRepository;
	private final ReferenceInfo2CodeItemMapper referenceInfo2CodeItemMapper;
	protected final NamespaceSearcher searcher;

	public FSResolver(final ExternalDatatypeInfoRepository datatypeInfoRepository,
			final NamespaceSearcher searcher) {
		this.datatypeInfoRepository = datatypeInfoRepository;
		this.searcher = searcher;
		this.referenceInfo2CodeItemMapper = new ReferenceInfo2CodeItemMapper();
	}

	public CodeItem fs_findItemTryingAllImports(final String identifier,
			final List<CodeItem> externalUsingDirectives) {
		for (CodeItem u : externalUsingDirectives) {
			CodeItem item = fs_findItem(identifier, u);
			if (item != null)
				return item;
		}
		return null;
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see mapping.code.Resolver#fs_findItem(java.lang.String,
	 * org.eclipse.gmt.modisco.omg.kdm.code.CodeItem)
	 */
	@Override
	public CodeItem fs_findItem(final String identifier, final CodeItem prefix)
			throws TypeNotFoundException {
		try {
			ReferenceInfo referenceInfo = datatypeInfoRepository.getReferenceInfo(prefix,
					identifier);
			CodeItem child = referenceInfo2CodeItemMapper.convert(referenceInfo);
			addToOwner(child, prefix);
			return child;
		} catch (FileNotFoundException e) {
			// do nothing; check next using directive
		} catch (DatatypeInfoLoadException e) {
			// do nothing; check next using directive
		}
		return null;
	}

	protected void addToOwner(final CodeItem child, final CodeItem parent) {
		// System.out.println("addToOwner.child: " + child);
		// System.out.println("addToOwner.parent: " + parent);

		if (parent instanceof ClassUnit) { // child: method, member, inner type
			((ClassUnit) parent).getCodeElement().add(child);
		} else if (parent instanceof InterfaceUnit) { // child: method, inner
			// type(?)
			((InterfaceUnit) parent).getCodeElement().add(child);
		} else if (parent instanceof CompositeType) {
			((CompositeType) parent).getItemUnit().add((ItemUnit) child);
		} else if (parent instanceof Namespace) {
			((Namespace) parent).getGroupedCode().add(child);
			if (child instanceof Namespace) {
				((Module) parent.eContainer()).getCodeElement().add(child);
			} else if (child instanceof Datatype) {
				CodeModel externalCodeModel = ((CodeModel) searcher.externalGlobalNamespace
						.eContainer().eContainer());
				externalCodeModel.getCodeElement().add(child);
			}
		} else {
			System.err.println("UNSUPPORTED addToOwner.parent: " + parent);
			Pause.pause();
		}
	}
}
