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
import java.util.ArrayList;
import java.util.Arrays;
import java.util.List;

import org.eclipse.core.runtime.IProgressMonitor;
import org.eclipse.gmt.modisco.omg.kdm.source.BinaryFile;
import org.eclipse.gmt.modisco.omg.kdm.source.Configuration;
import org.eclipse.gmt.modisco.omg.kdm.source.Directory;
import org.eclipse.gmt.modisco.omg.kdm.source.ExecutableFile;
import org.eclipse.gmt.modisco.omg.kdm.source.Image;
import org.eclipse.gmt.modisco.omg.kdm.source.InventoryContainer;
import org.eclipse.gmt.modisco.omg.kdm.source.InventoryItem;
import org.eclipse.gmt.modisco.omg.kdm.source.ResourceDescription;
import org.eclipse.gmt.modisco.omg.kdm.source.SourceFactory;
import org.eclipse.gmt.modisco.omg.kdm.source.SourceFile;

import util.FileAccess;
import util.FileListener;

public class InventoryModelFileListener implements FileListener {

	private InventoryContainer			parentContainer;
	private final SourceFactory			sourceFactory;
	private static final List<String>	IMAGE_EXTENSIONS		= new ArrayList<String>();
	private static final List<String>	SOURCECODE_EXTENSIONS	= new ArrayList<String>();
	private static final List<String>	CONFIGURATIONS			= new ArrayList<String>();
	private static final List<String>	RESOURCE_DESCRIPTIONS	= new ArrayList<String>();

	static {
		IMAGE_EXTENSIONS.addAll(Arrays.asList("jpg", "jpeg", "png", "bmp", "tif", "gif", "svg"));
		SOURCECODE_EXTENSIONS.addAll(Arrays.asList("java", "c", "cpp", "rb", "cs", "py"));
		CONFIGURATIONS.addAll(Arrays.asList("cfg", "ini", "xml", "prefs", "properties"));
		RESOURCE_DESCRIPTIONS.addAll(Arrays.asList("web.xml", "plugin.xml", "MANIFEST.MF"));
	}

	public InventoryModelFileListener(final InventoryContainer root) {
		this.parentContainer = root;
		this.sourceFactory = SourceFactory.eINSTANCE;
	}

	@Override
	public void updateFile(final File dir, final File file, final IProgressMonitor monitor) {
		//		monitor.subTask("Found file: " + file.getAbsolutePath());
		InventoryItem inventoryItem;
		// TODO for content type, see IContentTypeManager
		if (isImageFile(file)) {
			Image image = sourceFactory.createImage();

			inventoryItem = image;
		} else if (isConfigurationFile(file)) {
			Configuration configuration = sourceFactory.createConfiguration();

			inventoryItem = configuration;
		} else if (isExecutableFile(file)) {
			ExecutableFile executableFile = sourceFactory.createExecutableFile();

			inventoryItem = executableFile;
		} else if (isResourceDescriptionFile(file)) {
			ResourceDescription resourceDescription = sourceFactory.createResourceDescription();

			inventoryItem = resourceDescription;
		} else if (isSourceFile(file)) {
			SourceFile sourceFile = sourceFactory.createSourceFile();

			sourceFile.setEncoding("not checked");
			sourceFile.setLanguage(getLanguageFromFile(file));

			inventoryItem = sourceFile;
		} else { // fall back case: it's at least a binary file
			BinaryFile binaryFile = sourceFactory.createBinaryFile();

			inventoryItem = binaryFile;
		}

		// set common attributes
		inventoryItem.setName(file.getName());
		inventoryItem.setPath(file.getAbsolutePath());
		inventoryItem.setVersion(Long.toString(file.lastModified()));

		parentContainer.getInventoryElement().add(inventoryItem);
		monitor.worked(1);
	}

	@Override
	public void enterDir(final File parent, final File dir, final IProgressMonitor monitor) {
		//		monitor.subTask("Found directory: " + dir.getAbsolutePath());
		Directory directory = sourceFactory.createDirectory();

		directory.setName(dir.getName());
		directory.setPath(dir.getAbsolutePath());

		parentContainer.getInventoryElement().add(directory);
		parentContainer = directory;
	}

	@Override
	public void exitDir(final File parent, final File dir, final IProgressMonitor monitor) {
		parentContainer = (InventoryContainer) parentContainer.eContainer();
		monitor.worked(1);
	}

	private boolean isImageFile(final File file) {
		String fileExt = FileAccess.getFileExtension(file);
		return IMAGE_EXTENSIONS.contains(fileExt);
	}

	private boolean isConfigurationFile(final File file) {
		String fileExt = FileAccess.getFileExtension(file);
		return CONFIGURATIONS.contains(fileExt);
	}

	private boolean isExecutableFile(final File file) {
		// TODO add detection logic
		return false;
	}

	private boolean isResourceDescriptionFile(final File file) {
		return RESOURCE_DESCRIPTIONS.contains(file.getName());
	}

	private boolean isSourceFile(final File file) {
		String fileExt = FileAccess.getFileExtension(file);
		return SOURCECODE_EXTENSIONS.contains(fileExt);
	}

	private String getLanguageFromFile(final File file) {
		String fileExt = FileAccess.getFileExtension(file);
		// switch (fileExt) {
		// case "java":
		// return "Java";
		// case "rb":
		// return "Ruby";
		// case "cpp":
		// return "C++";
		// case "c":
		// return "C";
		// case "cs":
		// return "C#";
		// default:
		// return "Unknown";
		// }
		// TODO use jdk7
		if (fileExt.equals("cs")) {
			return "C#";
		}
		return "Unknown";
	}

}
