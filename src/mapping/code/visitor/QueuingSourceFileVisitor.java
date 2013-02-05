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

package mapping.code.visitor;

import java.util.concurrent.BlockingQueue;

import mapping.source.visitor.DefaultSourceFileVisitor;

import org.eclipse.gmt.modisco.omg.kdm.source.SourceFile;

/**
 * <code>QueuingSourceFileVisitor</code> represents a visitor that puts each visited object into its
 * queue.
 * 
 * @author chw
 */
public class QueuingSourceFileVisitor extends DefaultSourceFileVisitor {

	private final BlockingQueue<SourceFile>	blockingQueue;

	public QueuingSourceFileVisitor(final BlockingQueue<SourceFile> blockingQueue) {
		this.blockingQueue = blockingQueue;
	}

	@Override
	public void visitSourceFile(final SourceFile sourceFile) {
		blockingQueue.add(sourceFile);
	}

}
