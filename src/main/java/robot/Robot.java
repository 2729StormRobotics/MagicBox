package robot;

import com.ctre.phoenix.motorcontrol.ControlMode;
import com.ctre.phoenix.motorcontrol.can.TalonSRX;
import edu.wpi.first.wpilibj.GenericHID;
import edu.wpi.first.wpilibj.IterativeRobot;
import edu.wpi.first.wpilibj.XboxController;
import edu.wpi.first.wpilibj.smartdashboard.SmartDashboard;
import robot.subsystems.PDP;

public class Robot extends IterativeRobot {

    public double intakeSpeed;
    public double outputSpeed;
    public double elevatorSpeed;

    public XboxController xboxDrive;

    public static TalonSRX intakeLeft = new TalonSRX(Constants.PORT_INTAKE_LEFT);
    public static TalonSRX intakeRight = new TalonSRX(Constants.PORT_INTAKE_RIGHT);

    public static TalonSRX outputLeft = new TalonSRX(Constants.PORT_OUTPUT_LEFT);
    public static TalonSRX outputRight = new TalonSRX(Constants.PORT_OUTPUT_RIGHT);

    public static TalonSRX elevatorLeft = new TalonSRX(Constants.PORT_MOTOR_DRIVE_ELEVATOR_MAIN);
    public static TalonSRX elevatorRight = new TalonSRX(Constants.PORT_MOTOR_DRIVE_ELEVATOR_2);

    @Override
    public void robotInit() {
        xboxDrive = new XboxController(Constants.PORT_XBOX_DRIVE);
        intakeSpeed = 0.0;
        outputSpeed = 0.0;
        elevatorSpeed = 0.0;
        SmartDashboard.putNumber("Intake Speed", 0.0);
        SmartDashboard.putNumber("Output Speed", 0.0);
        SmartDashboard.putNumber("Elevator Speed", 0.0);
        intakeRight.follow(intakeLeft);
        outputRight.follow(outputLeft);
        elevatorRight.follow(elevatorLeft);
    }

    @Override
    public void disabledInit() { }


    @Override
    public void teleopInit() { }

    @Override
    public void testInit() { }


    @Override
    public void disabledPeriodic() {
        PDP.dashboardStats();
    }


    @Override
    public void teleopPeriodic() {
        //Smart Dashboard

        intakeSpeed = SmartDashboard.getNumber("Intake Speed", 0.0);
        outputSpeed = SmartDashboard.getNumber("Output Speed", 0.0);
        elevatorSpeed = SmartDashboard.getNumber("Elevator Speed", 0.0);


        //Remote

        //intakeSpeed = xboxDrive.getTriggerAxis(XboxController.Hand.kRight);

        //intakeLeft.set(ControlMode.PercentOutput, intakeSpeed);
      //  intakeRight.set(ControlMode.PercentOutput, intakeSpeed);
        //outputLeft.set(ControlMode.PercentOutput, outputSpeed);
        elevatorLeft.set(ControlMode.PercentOutput, elevatorSpeed);





        PDP.dashboardStats();
    }

    @Override
    public void testPeriodic() {
        PDP.dashboardStats();
    }
}
