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

package analysis;

import java.io.BufferedReader;
import java.io.FileNotFoundException;
import java.io.IOException;
import java.util.LinkedList;
import java.util.List;

import util.FileAccess;

public class PathLoader {

	private PathLoader() {
		// utility class
	}

	public static List<String> loadPaths(final String filename) throws FileNotFoundException {
		List<String> paths = new LinkedList<String>();

		BufferedReader stream = FileAccess.loadTextFileAsStream(filename);
		try {
			String line;
			while ((line = stream.readLine()) != null) {
				paths.add(line);
			}
			return paths;
		} catch (IOException e) {
			throw new IllegalStateException(e.getLocalizedMessage(), e);
		} finally {
			try {
				stream.close();
			} catch (IOException e) {
				throw new IllegalStateException(e.getLocalizedMessage(), e);
			}
		}
	}
}
