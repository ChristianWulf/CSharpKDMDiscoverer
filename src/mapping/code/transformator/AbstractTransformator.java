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

package mapping.code.transformator;

import mapping.code.extern.TypeNotFoundException;

import org.antlr.runtime.tree.Tree;
import org.eclipse.gmt.modisco.omg.kdm.code.CodeItem;

public abstract class AbstractTransformator {

	protected abstract Object walk(final Tree node, final CodeItem parent)
			throws TypeNotFoundException;

	@SuppressWarnings("unchecked")
	public <T> T walk(final Tree node, final CodeItem parent, final Class<T> class1)
			throws TypeNotFoundException {
		Object result = walk(node, parent);
		return ((T) result);
	}
}
