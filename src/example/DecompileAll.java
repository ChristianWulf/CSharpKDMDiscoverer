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

import java.io.File;
import java.io.IOException;
import java.util.Arrays;
import java.util.Scanner;

import org.eclipse.core.runtime.IProgressMonitor;
import org.eclipse.core.runtime.NullProgressMonitor;

import util.FileAccess;
import util.FileListener;

public class DecompileAll {

	final static String	JUST_DECOMPILE	= "C:/Program Files (x86)/Telerik/JustDecompile/Libraries/JustDecompileCmd.exe";
	final static String	NET_FRAMEWORK	= "C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.0";
	final static String	TARGET_DIR		= "D:/.net framework 4.0 decompiled";

	public static void main(final String[] args) {
		FileListener listener = new FileListener() {

			@Override
			public void updateFile(final File dir, final File file, final IProgressMonitor monitor) {
				if (file.getName().endsWith(".dll")) {
					String[] command = createCommand(file.getAbsolutePath());

					System.out.println("Executing " + Arrays.toString(command));
					ProcessBuilder cmd = new ProcessBuilder("cmd","/c","dir");
					try {
						Process process = cmd.start();
						process.waitFor();
						Scanner s = new Scanner( process.getInputStream() ).useDelimiter( "\\Z" );
						System.out.println( s.next() );
					} catch (IOException e) {
						// TODO Auto-generated catch block
						e.printStackTrace();
					} catch (InterruptedException e) {
						// TODO Auto-generated catch block
						e.printStackTrace();
					}
				}
			}

			@Override
			public void enterDir(final File parent, final File dir, final IProgressMonitor monitor) {
				// do nothing
			}

			@Override
			public void exitDir(final File parent, final File dir, final IProgressMonitor monitor) {
				// do nothing
			}
		};

		File dir = new File(NET_FRAMEWORK);
		FileAccess.walkDirectoryRecursively(dir, listener, new NullProgressMonitor());
	}

	protected static String[] createCommand(final String filePath) {
		String[] command = new String[3];
		command[0] = "cmd";
		command[1] = "/c";
		command[2] = "dir";
		//		command[1] = "/target";
		//		command[2] = "\"" + filePath + "\"";
		//		command[3] = "/out";
		//		command[4] = "\"" + TARGET_DIR + "\"";
		return command;
	}
}
