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

package mapping.source;

import java.io.File;

import mapping.IModelCreator;
import mapping.code.MoDiscoKDM;

import org.eclipse.core.runtime.IProgressMonitor;
import org.eclipse.core.runtime.NullProgressMonitor;
import org.eclipse.gmt.modisco.omg.kdm.source.Directory;
import org.eclipse.gmt.modisco.omg.kdm.source.InventoryModel;
import org.eclipse.gmt.modisco.omg.kdm.source.SourceFactory;

import util.FileAccess;

public final class InventoryModelCreator implements IModelCreator<InventoryModel> {

	private final SourceFactory sourceFactory;

	public InventoryModelCreator() {
		this.sourceFactory = SourceFactory.eINSTANCE;
	}

	@Override
	public InventoryModel openDirectory(final File directory, IProgressMonitor monitor) {
		// ensure there is a monitor of some sort
		if (monitor == null)
			monitor = new NullProgressMonitor();

		InventoryModel inventoryModel = sourceFactory.createInventoryModel();
		inventoryModel.setName(MoDiscoKDM.INVENTORYMODEL_NAME);

		final Directory root = sourceFactory.createDirectory();
		root.setName(directory.getName());
		root.setPath(directory.getAbsolutePath());

		inventoryModel.getInventoryElement().add(root);

		monitor.beginTask("Scanning directory..." + directory.getAbsolutePath(),
				IProgressMonitor.UNKNOWN);
		try {
			FileAccess.walkDirectoryRecursively(directory,
					new InventoryModelFileListener(root), monitor);
		} finally {
			monitor.done();
		}

		return inventoryModel;
	}

	@Override
	public InventoryModel openDirectory(final String dirname,
			final IProgressMonitor monitor) {
		return openDirectory(new File(dirname), monitor);
	}

}
