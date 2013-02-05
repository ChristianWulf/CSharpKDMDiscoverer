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

import java.util.ArrayList;
import java.util.List;

import org.antlr.runtime.tree.CommonTree;
import org.antlr.runtime.tree.Tree;
import org.eclipse.gmt.modisco.omg.kdm.source.SourceFactory;
import org.eclipse.gmt.modisco.omg.kdm.source.SourceRef;
import org.eclipse.gmt.modisco.omg.kdm.source.SourceRegion;

public class TreeHelper {

	private TreeHelper() {
		// utility class
	}

	public static List<String> treeListToStringList(final List<Tree> identifiers) {
		List<String> stringList = new ArrayList<String>();
		for (Tree ct : identifiers) {
			stringList.add(ct.getText());
		}
		return stringList;
	}

	public static SourceRef getCorrespondingSourceRef(final CommonTree commonTreeNode) {
		SourceRegion sourceRegion = SourceFactory.eINSTANCE.createSourceRegion();
		sourceRegion.setStartLine(commonTreeNode.getToken().getLine());
		sourceRegion.setStartPosition(commonTreeNode.getToken().getCharPositionInLine());

		SourceRef sourceRef = SourceFactory.eINSTANCE.createSourceRef();
		sourceRef.getRegion().add(sourceRegion);
		return sourceRef;
	}
}
