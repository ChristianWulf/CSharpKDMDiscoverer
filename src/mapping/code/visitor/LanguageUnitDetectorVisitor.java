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

package mapping.code.visitor;

import java.util.ArrayList;
import java.util.Collection;
import java.util.HashMap;
import java.util.Map;

import mapping.code.language.CSharpLanguageUnitCache;
import mapping.code.language.GenericLanguageUnitCache;
import mapping.source.visitor.DefaultSourceFileVisitor;

import org.eclipse.gmt.modisco.omg.kdm.code.CodeFactory;
import org.eclipse.gmt.modisco.omg.kdm.code.LanguageUnit;
import org.eclipse.gmt.modisco.omg.kdm.source.SourceFile;

import util.Logger;

// TODO check for thread-safe
public class LanguageUnitDetectorVisitor extends DefaultSourceFileVisitor {

	private static final Logger				LOGGER = new Logger(LanguageUnitDetectorVisitor.class);

	private static final CodeFactory		CODE_FACTORY;

	static {
		CODE_FACTORY = CodeFactory.eINSTANCE;
	}

	private Map<String, GenericLanguageUnitCache>	genericLanguageUnitCaches;

	private boolean							walkedThrough;

	@Override
	public void beforeWalk() {
		genericLanguageUnitCaches = new HashMap<String, GenericLanguageUnitCache>();
	}

	@Override
	public void visitSourceFile(final SourceFile sourceFile) {
		String language = sourceFile.getLanguage();
		if (!"Unknown".equals(language)) {
			if (!genericLanguageUnitCaches.containsKey(language)) {
				GenericLanguageUnitCache genericLanguageUnitCache = newLanguageUnit(language);
				genericLanguageUnitCaches.put(language, genericLanguageUnitCache);
			}
		}
	}

	@Override
	public void afterWalk() {
		walkedThrough = true;
	}

	public GenericLanguageUnitCache getCache(final String language) {
		checkState();
		return genericLanguageUnitCaches.get(language);
	}

	/**
	 * Searches for and returns all <code>LanguageUnit</code>s necessary to represent the underlying
	 * software system.
	 * 
	 * @return all necessary <code>LanguageUnit</code>s
	 */
	public Collection<LanguageUnit> getNecessaryLanguageUnits() {
		checkState();
		Collection<LanguageUnit> units = new ArrayList<LanguageUnit>();
		for (GenericLanguageUnitCache luc : genericLanguageUnitCaches.values()) {
			units.add(luc.getLanguageUnit());
		}
		return units;
	}

	private void checkState() {
		if (!walkedThrough) {
			throw new IllegalStateException("This visitor have not yet visited a walker.");
		}
	}

	private GenericLanguageUnitCache newLanguageUnit(final String language) {
		GenericLanguageUnitCache genericLanguageUnitCache;

		if (language.equals("C#")) {
			genericLanguageUnitCache = new CSharpLanguageUnitCache();
		} else {
			genericLanguageUnitCache = new GenericLanguageUnitCache(CODE_FACTORY.createLanguageUnit());
			genericLanguageUnitCache.setName(language);
			LOGGER.warning("A LanguageUnit for " + language + " is not supported.");
		}
		return genericLanguageUnitCache;
	}
}
