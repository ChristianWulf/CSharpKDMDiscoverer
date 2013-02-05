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

package example;

import java.awt.Dimension;

import javax.swing.JFrame;
import javax.swing.JProgressBar;
import javax.swing.ProgressMonitor;
import javax.swing.SwingUtilities;
import javax.swing.UIManager;

import org.eclipse.core.runtime.IProgressMonitor;

public class ProgressMonitorExample extends JFrame implements IProgressMonitor {
	private static final long serialVersionUID = 1L;

	ProgressMonitor pbar;
	int worked;

	private String currentSubTask;

	private final JProgressBar progressBar;

	static {
		UIManager.put("ProgressMonitor.progressText", "Progress");
		UIManager.put("OptionPane.cancelButtonText", "Cancel");
	}

	public ProgressMonitorExample() {
		super("Progress Monitor Demo");
		setDefaultCloseOperation(JFrame.EXIT_ON_CLOSE);
		setPreferredSize(new Dimension(400, 100));

		progressBar = new JProgressBar();
		// add(progressBar);

		pbar = new ProgressMonitor(this, "", "Initializing...", 0, 100);
		pbar.setMillisToDecideToPopup(0);
		pbar.setMillisToPopup(0);
		// setVisible(true);
	}

	public static void main(final String args[]) {
		new ProgressMonitorExample();
	}

	@Override
	public void beginTask(final String name, final int totalWork) {
		SwingUtilities.invokeLater(new Runnable() {
			@Override
			public void run() {
				ProgressMonitorExample.this.setTitle(name);
				pbar.setMinimum(0);
				pbar.setMaximum(totalWork);
				progressBar.setMinimum(0);
				progressBar.setMaximum(totalWork);
				System.out.println(name + "(" + totalWork + ")");
			}
		});
	}

	@Override
	public void done() {
		SwingUtilities.invokeLater(new Runnable() {
			@Override
			public void run() {
				ProgressMonitorExample.this.dispose();
			}
		});
	}

	@Override
	public void internalWorked(final double arg0) {
	}

	@Override
	public boolean isCanceled() {
		return pbar.isCanceled();
	}

	@Override
	public void setCanceled(final boolean arg0) {
	}

	@Override
	public void setTaskName(final String name) {
		SwingUtilities.invokeLater(new Runnable() {
			@Override
			public void run() {
				pbar.setNote(name);
			}
		});
	}

	@Override
	public void subTask(final String name) {
		currentSubTask = name;
		SwingUtilities.invokeLater(new Runnable() {
			@Override
			public void run() {
				pbar.setNote(currentSubTask);
				System.out.println(currentSubTask);
			}
		});
	}

	@Override
	public void worked(final int work) {
		SwingUtilities.invokeLater(new Runnable() {
			@Override
			public void run() {
				worked += work;
				pbar.setProgress(worked);
				pbar.setNote("(" + worked + "/" + pbar.getMaximum() + ") "
						+ currentSubTask);
			}
		});
	}
}
