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

package mapping.source.visitor;

import org.eclipse.gmt.modisco.omg.kdm.code.CodeModel;
import org.eclipse.gmt.modisco.omg.kdm.source.SourceFile;

public class DefaultSourceFileVisitor implements SourceFileVisitor {

	@Override
	public void beforeWalk() {
		// default implementation: empty
	}

	@Override
	public void visitSourceFile(final SourceFile sourceFile) {
		// default implementation: empty
	}

	@Override
	public void afterWalk() {
		// default implementation: empty
	}

	@Override
	public CodeModel getInternalCodeModel() {
		// default implementation: return null
		return null;
	}

	@Override
	public CodeModel getExternalCodeModel() {
		// default implementation: return null
		return null;
	}

}