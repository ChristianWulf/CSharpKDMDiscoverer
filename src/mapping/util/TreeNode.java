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

import java.util.LinkedList;
import java.util.List;

@Deprecated
public class TreeNode<S1, S2> {

	private final List<S1>	childNodes	= new LinkedList<S1>();
	private final List<S2>	childLeaves	= new LinkedList<S2>();
	private S1	content;

	public List<S1> getChildNodes() {
		return childNodes;
	}

	public List<S2> getChildLeafs() {
		return childLeaves;
	}

	void setNodeContent(final S1 content) {
		this.content = content;
	}

	public S1 getNodeContent() {
		return content;
	}

	void addChildNode(final S1 node) {
		childNodes.add(node);
	}

	void addChildLeaf(final S2 leaf) {
		childLeaves.add(leaf);
	}
}
