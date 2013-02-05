package org.eclipse.modisco.csharp.discoverer.internal;

import java.io.File;
import java.util.Comparator;

public class FileComparator implements Comparator<File> {

	@Override
	public int compare(final File arg0, final File arg1) {
		long size0 = arg0.length();
		long size1 = arg1.length();
		return size0 > size1 ? 1 : (size0 < size1 ? -1 : 0);
	}

}
