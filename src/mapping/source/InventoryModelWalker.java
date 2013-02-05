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

import java.util.List;

import mapping.source.visitor.SourceFileVisitor;

import org.eclipse.gmt.modisco.omg.kdm.source.AbstractInventoryElement;
import org.eclipse.gmt.modisco.omg.kdm.source.InventoryContainer;
import org.eclipse.gmt.modisco.omg.kdm.source.InventoryModel;
import org.eclipse.gmt.modisco.omg.kdm.source.SourceFile;

public class InventoryModelWalker {

	private final InventoryModel	inventoryModel;

	public InventoryModelWalker(final InventoryModel inventoryModel) {
		this.inventoryModel = inventoryModel;
	}

	public void walk(final SourceFileVisitor visitor) {
		visitor.beforeWalk();
		walk(inventoryModel.getInventoryElement(), visitor);
		visitor.afterWalk();
	}

	private void walk(final List<AbstractInventoryElement> elements, final SourceFileVisitor visitor) {
		for (AbstractInventoryElement inventoryElement : elements) {
			if (inventoryElement instanceof InventoryContainer) {
				walk(((InventoryContainer) inventoryElement).getInventoryElement(), visitor);
			} else if (inventoryElement instanceof SourceFile) {
				visitor.visitSourceFile((SourceFile) inventoryElement);
			}
		}
	}

}