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

package de.cau.chw.csharp.resolution;

import java.util.HashMap;
import java.util.Map;
import java.util.Map.Entry;

import mapping.KDMElementFactory;

import org.eclipse.gmt.modisco.omg.kdm.code.ClassUnit;
import org.eclipse.gmt.modisco.omg.kdm.code.Datatype;
import org.eclipse.gmt.modisco.omg.kdm.code.EnumeratedType;
import org.eclipse.gmt.modisco.omg.kdm.code.InterfaceUnit;
import org.eclipse.gmt.modisco.omg.kdm.code.Namespace;
import org.eclipse.gmt.modisco.omg.kdm.core.KDMEntity;

/**
 * @author chw
 */
public class Mapper {

	final Map<String, KDMEntity>	externLibraryElement	= new HashMap<String, KDMEntity>();

	final Map<String, KDMEntity>	usingDirectives			= new HashMap<String, KDMEntity>();

	final Map<String, KDMEntity>	namespaces				= new HashMap<String, KDMEntity>();
	final Map<String, KDMEntity>	classes					= new HashMap<String, KDMEntity>();
	final Map<String, KDMEntity>	enums					= new HashMap<String, KDMEntity>();
	final Map<String, KDMEntity>	interfaces				= new HashMap<String, KDMEntity>();
	final Map<String, KDMEntity>	delegates				= new HashMap<String, KDMEntity>();

	/**
	 * @param usingDirective
	 * @return null, a ClassUnit, or a NamespaceUnit
	 */
	public KDMEntity getUsingDirective(final String usingDirective) {
		return usingDirectives.get(usingDirective);
	}

	//
	// public KDMEntity getClassUnit(String className) {
	//
	// }

	public void initClassFilter() {
		classes.clear();
		for (Entry<String, KDMEntity> entry : externLibraryElement.entrySet()) {
			if (entry.getValue() instanceof ClassUnit) {
				classes.put(entry.getKey(), entry.getValue());
			}
		}
	}

	public void initEnumFilter() {
		enums.clear();
		for (Entry<String, KDMEntity> entry : externLibraryElement.entrySet()) {
			if (entry.getValue() instanceof EnumeratedType) {
				enums.put(entry.getKey(), entry.getValue());
			}
		}
	}

	public void initInterfaceFilter() {
		interfaces.clear();
		for (Entry<String, KDMEntity> entry : externLibraryElement.entrySet()) {
			if (entry.getValue() instanceof InterfaceUnit) {
				interfaces.put(entry.getKey(), entry.getValue());
			}
		}
	}

	public void initDelegateFilter() {
		delegates.clear();
		for (Entry<String, KDMEntity> entry : externLibraryElement.entrySet()) {
			if (entry.getValue() instanceof Datatype && KDMElementFactory.isDelegateType(entry.getValue())) {
				delegates.put(entry.getKey(), entry.getValue());
			}
		}
	}

	public void initNamespaceFilter() {
		namespaces.clear();
		for (Entry<String, KDMEntity> entry : externLibraryElement.entrySet()) {
			if (entry.getValue() instanceof Namespace) {
				namespaces.put(entry.getKey(), entry.getValue());
			}
		}
	}
}
