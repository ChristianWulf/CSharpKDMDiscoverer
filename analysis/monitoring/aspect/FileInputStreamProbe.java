package monitoring.aspect;

import kieker.monitoring.probe.aspectj.flow.operationCall.AbstractAspect;

import org.aspectj.lang.annotation.Aspect;
import org.aspectj.lang.annotation.Pointcut;

@Aspect
public class FileInputStreamProbe extends AbstractAspect {

	// (call(* java.io.FileInputStream.*(..)) && noGetterAndSetter()) ||
	// call(java.io.FileInputStream.new(..)) ||
	// call(* java.io.File.*(..))
	// execution(* java.io.FileInputStream.read(..)) || execution(* java.io.FileInputStream.write(..)) || call(java.io.FileInputStream.new(..))
	@Pointcut("execution(* java.io.FileInputStream.read(..))")
	@Override
	public void monitoredOperation() {
		// empty
	}

}
