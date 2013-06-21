/***************************************************************************
 * Copyright 2012 by
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

package mapping.util;

import java.util.Collections;
import java.util.LinkedList;
import java.util.List;

import org.eclipse.gmt.modisco.omg.kdm.code.AbstractCodeElement;
import org.eclipse.gmt.modisco.omg.kdm.code.ClassUnit;
import org.eclipse.gmt.modisco.omg.kdm.code.CodeItem;
import org.eclipse.gmt.modisco.omg.kdm.code.CodeModel;
import org.eclipse.gmt.modisco.omg.kdm.code.CompilationUnit;
import org.eclipse.gmt.modisco.omg.kdm.code.CompositeType;
import org.eclipse.gmt.modisco.omg.kdm.code.ControlElement;
import org.eclipse.gmt.modisco.omg.kdm.code.Datatype;
import org.eclipse.gmt.modisco.omg.kdm.code.EnumeratedType;
import org.eclipse.gmt.modisco.omg.kdm.code.InterfaceUnit;
import org.eclipse.gmt.modisco.omg.kdm.code.MethodUnit;
import org.eclipse.gmt.modisco.omg.kdm.code.Namespace;
import org.eclipse.gmt.modisco.omg.kdm.code.Signature;
import org.eclipse.gmt.modisco.omg.kdm.code.TemplateUnit;
import org.eclipse.gmt.modisco.omg.kdm.source.SourceFile;
import org.eclipse.gmt.modisco.omg.kdm.source.SourceRef;
import org.eclipse.gmt.modisco.omg.kdm.source.SourceRegion;

import util.Logger;

public class KDMChildHelper {

	private static final Logger	LOG	= new Logger(KDMChildHelper.class);

	private KDMChildHelper() {
		// utility class
	}

	public static <T> T getChildItemByName(final String ident, final CodeItem parent, final Class<T> clazz) {
		List<? extends AbstractCodeElement> elements = getChildrenFromCodeItem(parent);
		return getChildItemByName(ident, elements, clazz);
	}

	/**
	 * @param ident
	 * @param elements
	 * @param clazz
	 * @return <code>null</code> if child could not be found
	 */
	@SuppressWarnings("unchecked")
	public static <T> T getChildItemByName(final String ident, final List<? extends AbstractCodeElement> elements,
			final Class<T> clazz) {
		for (AbstractCodeElement ace : elements) {
			if (clazz.isAssignableFrom(ace.getClass()) && ident.equals(ace.getName())) {
				return (T) ace;
			}
		}
		return null;
	}

	public static <T> T getChildItemByType(final CodeItem parent, final Class<T> clazz) {
		List<? extends AbstractCodeElement> elements = getChildrenFromCodeItem(parent);
		return getChildItemByType(elements, clazz);
	}

	@SuppressWarnings("unchecked")
	public static <T> T getChildItemByType(final List<? extends AbstractCodeElement> elements, final Class<T> clazz) {
		for (AbstractCodeElement ace : elements) {
			if (clazz.isAssignableFrom(ace.getClass())) return (T) ace;
		}
		return null;
	}

	public static List<? extends AbstractCodeElement> getChildrenFromCodeItem(final CodeItem parent) {
		if (parent instanceof Namespace) {
			return ((Namespace) parent).getGroupedCode();
		} else if (parent instanceof ClassUnit) {
			return ((ClassUnit) parent).getCodeElement();
		} else if (parent instanceof InterfaceUnit) {
			return ((InterfaceUnit) parent).getCodeElement();
		} else if (parent instanceof EnumeratedType) {
			return ((EnumeratedType) parent).getCodeElement();
		} else if (parent instanceof TemplateUnit) {
			return ((TemplateUnit) parent).getCodeElement();
		} else if (parent instanceof CompositeType) {
			return ((CompositeType) parent).getItemUnit();
		} else if (parent instanceof ControlElement) {
			return ((ControlElement) parent).getCodeElement();
		} else if (parent instanceof Datatype) {
			return Collections.emptyList();
		}
		throw new IllegalStateException("parent = " + parent.getClass());
	}

	public static MethodUnit getChildMethodByName(final String identifier, final CodeItem parent) {
		// System.out.println("KDMChildHelper.getChildMethodByName() "
		// + identifier + "," + parent);
		List<? extends AbstractCodeElement> elements = getChildrenFromCodeItem(parent);
		// System.out.println("KDMChildHelper.getChildMethodByName() " +
		// elements);
		for (AbstractCodeElement ace : elements) {
			// System.out.println("Child of " + parent.getName() + ": " + ace
			// + ", type = " + ace.getClass());

			if (ace instanceof TemplateUnit) { // unpack TemplateUnit
				TemplateUnit tunit = (TemplateUnit) ace;
				ace = KDMChildHelper.getChildItemByType(tunit, MethodUnit.class);
				if (ace == null) continue;
			}
			if (ace instanceof MethodUnit) {
				MethodUnit m = (MethodUnit) ace;
				Signature signature = getChildItemByType(m, Signature.class);
				// TODO support method loading on demand, e.g. List.Add(T)
				if (signature == null) return null;
				// System.out.println("signature: " + signature);
				String methodName = signature.getName();
				// System.out.println("methodName: " + methodName);
				String key = KeyStringHelper.buildSignatureKey(methodName, signature.getParameterUnit());
				// System.out.println("Checking method key: " + identifier
				// + " == (available) " + key + " ?");
				if (identifier.equals(key)) return m;
			}
		}
		return null;
	}

	/**
	 * Also searches in children of a child
	 * 
	 * @param elements
	 * @param name
	 * @param clazz
	 * @return
	 */
	@SuppressWarnings("unchecked")
	public static <T> T getAbstractCodeElementByName(final List<? extends AbstractCodeElement> elements,
			final String name, final Class<T> clazz) {
		for (AbstractCodeElement el : elements) {
			if (clazz.isAssignableFrom(el.getClass()) && name.equals(el.getName())) return (T) el;
			if (el instanceof ClassUnit) {
				try {
					return getAbstractCodeElementByName(
							((ClassUnit) el).getCodeElement(), name, clazz);
				} catch (IllegalStateException e) {
					// do nothing; proceed
				}
			} else if (el instanceof TemplateUnit) {
				try {
					return getAbstractCodeElementByName(
							((TemplateUnit) el).getCodeElement(), name, clazz);
				} catch (IllegalStateException e) {
					// do nothing; proceed
				}
			}
		}
		throw new IllegalStateException("Could not find " + name);
	}

	public static CompilationUnit getCompilationUnit(final SourceFile sourceFile, final CodeModel internalCodeModel) {
		for (AbstractCodeElement c : internalCodeModel.getCodeElement()) {
			if (c instanceof CompilationUnit) {
				for (SourceRef ref : c.getSource()) {
					for (SourceRegion r : ref.getRegion()) {
						if (r.getFile() == sourceFile) {
							return (CompilationUnit) c;
						}
					}
				}
			}
		}
		throw new IllegalStateException();
	}

	@SuppressWarnings("unchecked")
	public static <T, C> List<C> filterChildrenByType(final List<T> elements, final Class<C> clazz) {
		List<C> filteredElements = new LinkedList<C>();
		for (T ace : elements) {
			if (clazz.isAssignableFrom(ace.getClass())) filteredElements.add((C) ace);
		}
		return filteredElements;
	}
}
