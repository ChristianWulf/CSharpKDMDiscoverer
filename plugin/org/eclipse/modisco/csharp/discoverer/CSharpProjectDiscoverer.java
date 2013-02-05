package org.eclipse.modisco.csharp.discoverer;

import org.eclipse.core.resources.IProject;
import org.eclipse.core.runtime.IProgressMonitor;
import org.eclipse.modisco.infra.discovery.core.AbstractModelDiscoverer;
import org.eclipse.modisco.infra.discovery.core.exception.DiscoveryException;

public class CSharpProjectDiscoverer extends AbstractModelDiscoverer<IProject> {

	public static final String ID = "org.eclipse.modisco.csharp.discoverer.project"; //$NON-NLS-1$

	@Override
	public boolean isApplicableTo(final IProject source) {
		return source.isAccessible() /*&& TODO source.hasNature(JavaCore.NATURE_ID)*/;
	}

	@Override
	protected void basicDiscoverElement(final IProject source, final IProgressMonitor monitor)
			throws DiscoveryException {
		// TODO Auto-generated method stub

	}

}
