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

package mapping.code.extern;

import java.io.IOException;
import java.util.List;
import java.util.Map;

import mapping.code.extern.entity.DatatypeInfo;
import mapping.code.extern.reference.ReferenceInfo;

import org.antlr.runtime.RecognitionException;
import org.antlr.runtime.tree.CommonTree;

import util.Parsing;

public class DatatypeInfoSourceFileParser {

	private final List<DatatypeInfo>			definedClasses;
	private final Map<String, ReferenceInfo>	references;

	public DatatypeInfoSourceFileParser(final String filename) throws IOException,
	RecognitionException {
		CommonTree ast = Parsing.loadASTFromFile(filename);
		DatatypeInfoMapper datatypeInfoMapper = new DatatypeInfoMapper();
		datatypeInfoMapper.transform(ast);

		definedClasses = datatypeInfoMapper.getMappingResult();
		references = datatypeInfoMapper.getReferences();
	}

	@Deprecated
	public List<DatatypeInfo> getDefinedClasses() {
		return definedClasses;
	}

	@Deprecated
	public DatatypeInfo getFirstDefinedClass() {
		if (definedClasses.isEmpty()) {
			throw new IllegalStateException("This source file defines no classes/interfaces.");
		}
		return definedClasses.get(0);
	}

	public Map<String, ReferenceInfo> getReferences() {
		return references;
	}
}
