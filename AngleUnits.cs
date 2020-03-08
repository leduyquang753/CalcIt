using static CalcItUWP.Utils;

namespace CalcItUWP {
	public class AngleUnits {
		public static readonly AngleUnit DEGREE = new Degree();
		public static readonly AngleUnit RADIAN = new Radian();
		public static readonly AngleUnit GRADIAN = new Gradian();

		public interface AngleUnit {
			double convertToDegrees(double angle);
			double convertFromDegrees(double angle);
			double convertToRadians(double angle);
			double convertFromRadians(double angle);
			double convertToGradians(double angle);
			double convertFromGradians(double angle);
		}

		public class Degree: AngleUnit {
			public double convertToDegrees(double angle) => angle;
			public double convertFromDegrees(double angle) => angle;
			public double convertToRadians(double angle) => degToRad(angle);
			public double convertFromRadians(double angle) => radToDeg(angle);
			public double convertToGradians(double angle) => degToGrad(angle);
			public double convertFromGradians(double angle) => gradToDeg(angle);
		}

		public class Radian: AngleUnit {
			public double convertToDegrees(double angle) => radToDeg(angle);
			public double convertFromDegrees(double angle) => degToRad(angle);
			public double convertToRadians(double angle) => angle;
			public double convertFromRadians(double angle) => angle;
			public double convertToGradians(double angle) => radToGrad(angle);
			public double convertFromGradians(double angle) => gradToRad(angle);
		}

		public class Gradian: AngleUnit {
			public double convertToDegrees(double angle) => gradToDeg(angle);
			public double convertFromDegrees(double angle) => degToGrad(angle);
			public double convertToRadians(double angle) => gradToRad(angle);
			public double convertFromRadians(double angle) => radToGrad(angle);
			public double convertToGradians(double angle) => angle;
			public double convertFromGradians(double angle) => angle;
		}
	}
}
