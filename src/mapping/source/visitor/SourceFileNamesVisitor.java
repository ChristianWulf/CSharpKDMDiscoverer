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

package mapping.source.visitor;

import java.util.LinkedList;
import java.util.List;

import org.eclipse.gmt.modisco.omg.kdm.source.SourceFile;

/**
 * @author chw
 */
public class SourceFileNamesVisitor extends SourceFileCounter {

	protected List<String>	sourceFileNames;

	public SourceFileNamesVisitor(final String language) {
		super(language);
	}

	@Override
	public void visitSourceFile(final SourceFile sourceFile) {
		super.visitSourceFile(sourceFile);
		if (language != null && !sourceFile.getLanguage().equals(language)) return;
		sourceFileNames.add(sourceFile.getPath());
	}

	@Override
	public void beforeWalk() {
		super.beforeWalk();
		sourceFileNames = new LinkedList<String>();
	}

	public List<String> getSourceFileNames() {
		return sourceFileNames;
	}
}
