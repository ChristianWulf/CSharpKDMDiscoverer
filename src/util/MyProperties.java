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

package util;

import java.io.File;
import java.io.FileInputStream;
import java.io.IOException;
import java.net.URISyntaxException;
import java.net.URL;
import java.util.Properties;

import org.eclipse.core.runtime.FileLocator;
import org.eclipse.core.runtime.Platform;
import org.osgi.framework.Bundle;
import org.osgi.framework.BundleContext;

public final class MyProperties extends Properties {

	private static final long serialVersionUID = -4274610920021511038L;
	private static final String CONFIG_FILENAME = "META-INF/transformation.properties";

	/**
	 * Constructor for plug-ins with bundle identifier
	 * 
	 * @param bundleId
	 * @throws IOException
	 */
	public MyProperties(final String bundleId) throws IOException {
		Bundle bundle = Platform.getBundle(bundleId);
		File configFile = getFileFromBundle(CONFIG_FILENAME, bundle);
		loadFile(configFile);
	}

	/**
	 * Constructor for plug-ins without bundle identifier
	 * 
	 * @param bundleContext
	 * @throws IOException
	 */
	public MyProperties(final BundleContext bundleContext) throws IOException {
		Bundle bundle = bundleContext.getBundle();
		File configFile = getFileFromBundle(CONFIG_FILENAME, bundle);
		loadFile(configFile);
	}

	/**
	 * Constructor for standard java applications
	 * 
	 */
	public MyProperties() {
		File configFile = new File(CONFIG_FILENAME);
		loadFile(configFile);
	}

	private void loadFile(final File configFile) {
		try {
			FileInputStream fis = new FileInputStream(configFile);
			try {
				load(fis);
				System.out.println("Properties successfully loaded:\n\t" + this);
			} catch (IOException e) {
				e.printStackTrace();
			} finally {
				try {
					fis.close();
				} catch (IOException e) {
					throw new IllegalStateException(e.getLocalizedMessage(), e);
				}
			}
		} catch (IOException e) {
			e.printStackTrace();
		}
	}

	private static File getFileFromBundle(final String filename, final Bundle bundle)
			throws IOException {
		URL fileURL = bundle.getResource("/" + filename); //$NON-NLS-1$
		fileURL = FileLocator.toFileURL(fileURL);

		java.net.URI uri;
		try {
			uri = fileURL.toURI();
		} catch (URISyntaxException e) {
			throw new IllegalStateException(e.getLocalizedMessage(), e);
		}

		return new File(uri);
	}

}
