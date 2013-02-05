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

package mapping.util;

import java.util.Collection;
import java.util.Properties;

import mapping.KDMElementFactory;
import mapping.code.CodeModelCreator;

import org.apache.logging.log4j.LogManager;
import org.apache.logging.log4j.Logger;
import org.eclipse.core.runtime.IProgressMonitor;
import org.eclipse.gmt.modisco.omg.kdm.code.CodeModel;
import org.eclipse.gmt.modisco.omg.kdm.code.LanguageUnit;
import org.eclipse.gmt.modisco.omg.kdm.kdm.Segment;
import org.eclipse.gmt.modisco.omg.kdm.source.InventoryModel;

public class ModelCreationHelper {

	private static final Logger LOGGER = LogManager.getLogger(ModelCreationHelper.class);

	private ModelCreationHelper() {
		// utility class
	}

	public static Segment buildKDMInstance(
			final InventoryModel inventoryModel, final IProgressMonitor monitor, final Properties prop, final int toPhase) {
		CodeModelCreator creator = new CodeModelCreator(prop, toPhase);
		try {
			creator.buildCodeModel(inventoryModel, monitor);
		} catch (StackOverflowError e) {
			LOGGER.error("Could not build model due to StackOverflowError. "
					+ "Increase the default stack size limit for threads by the JVM argument '-Xss'.", e);
			e.printStackTrace();
		}

		CodeModel internalCodeModel = creator.getInternalCodeModel();
		CodeModel externalCodeModel = creator.getExternalCodeModel();
		Collection<LanguageUnit> neccessaryLanguageUnits = creator
				.getNeccessaryLanguageUnits();

		internalCodeModel.getCodeElement().addAll(neccessaryLanguageUnits);

		Segment segment = KDMElementFactory.createSegment();
		segment.getModel().add(inventoryModel);
		segment.getModel().add(internalCodeModel);
		segment.getModel().add(externalCodeModel);

		return segment;
	}
}
