import numpy as np
import cv2
import glob
import Triangulation as tri
import ImageRectification as ImRec


class HLCameraCalibration:
    
    def OpenCV_Chess_Calibration(left_cam_path, right_cam_path):
        '''
        Summary:
        Function is used to calibrate the front stereo cameras on the
        HoloLens 2 device. Upon completion of the calibration, an 'xml'
        file is created which contains the calibration parameters for
        the cameras. NOTE if a calibration has already been completed,
        the existing parameters can be used to run the body tracking app.

        Parameters:\n
        *_cam_path >> the folder path to where the calibration images are stored
        '''

        print("HL Camera Calibration :: Calibrating...\n")
        
        ########## Find chessboard corners (object/image points) ###########
        chessboardSize = (7,6)
        frameSize = (640,480)

        # termination criteria
        criteria = (cv2.TERM_CRITERIA_EPS + cv2.TERM_CRITERIA_MAX_ITER, 30, 0.001)

        # prepare object points, like (0,0,0), (1,0,0), (2,0,0) ....,(6,5,0)
        objp = np.zeros((6*7,3), np.float32)
        objp[:,:2] = np.mgrid[0:7,0:6].T.reshape(-1,2)

        # Arrays to store object points and image points from all the images.
        objpoints = [] # 3d point in real world space
        imgpointsL = [] # 2d points in LEFT image plane.
        imgpointsR = [] # 2d points in RIGHT image plane.

        imagesLeft = glob.glob(left_cam_path + '*.png')
        imagesRight = glob.glob(right_cam_path + '*.png')

        print('Detecting chessboard corners...')
        valid_img_count = 0
        total_img_count = 0
        for imgLeft, imgRight in zip(imagesLeft, imagesRight):
            imgL = cv2.imread(imgLeft)
            imgR = cv2.imread(imgRight)
            grayL = cv2.cvtColor(imgL, cv2.COLOR_BGR2GRAY)
            grayR = cv2.cvtColor(imgR, cv2.COLOR_BGR2GRAY)
            total_img_count += 2

            retL, cornersL = cv2.findChessboardCorners(grayL, chessboardSize, None)
            retR, cornersR = cv2.findChessboardCorners(grayR, chessboardSize, None)

            # check whether the calibration images are valid or not
            if retL and retR == True:
                valid_img_count += 2
                objpoints.append(objp)

                cornersL = cv2.cornerSubPix(grayL, cornersL, (11,11), (-1,-1), criteria)
                imgpointsL.append(cornersL)

                cornersR = cv2.cornerSubPix(grayR, cornersR, (11,11), (-1,-1), criteria)
                imgpointsR.append(cornersR)

        print(f'Valid calibration images >> ({valid_img_count}/{total_img_count})\n')

        ########## Calibration ###########
        print('Getting optimal camera matrices...')
        retL, cameraMartrixL, distL, rvecsL, tvecsL = cv2.calibrateCamera(objpoints, imgpointsL, grayL.shape[::-1], None, None)
        heightL, widthL, channelsL = imgL.shape
        newCameraMatrixL, roi_L = cv2.getOptimalNewCameraMatrix(cameraMartrixL, distL, (widthL, heightL), 1, (widthL, heightL))

        retR, cameraMartrixR, distR, rvecsR, tvecsR = cv2.calibrateCamera(objpoints, imgpointsR, grayR.shape[::-1], None, None)
        heightR, widthR, channelsR = imgR.shape
        newCameraMatrixR, roi_R = cv2.getOptimalNewCameraMatrix(cameraMartrixR, distR, (widthR, heightR), 1, (widthR, heightR))

        ########## Stereo Vision Calibration ###########
        print('Calibrating stereo cameras...')
        flags = 0
        flags |= cv2.CALIB_FIX_INTRINSIC

        criteria_stereo = (cv2.TERM_CRITERIA_EPS + cv2.TERM_CRITERIA_MAX_ITER, 30, 0.001)

        retStereo, newCameraMatrixL, distL, newCameraMatrixR, distR, rot, trans, essentialMatrix, fundamentalMatrix = cv2.stereoCalibrate(objpoints, imgpointsL, imgpointsR, newCameraMatrixL, distL, newCameraMatrixR, distR, grayL.shape[::-1], criteria_stereo, flags)

        ########## Stereo Rectification ###########
        print('Getting stereo rectification parameters...')
        rectifyScale = 1
        rectL, rectR, projMatrixL, projMatrixR, Q, roi_L, roi_R = cv2.stereoRectify(newCameraMatrixL, distL, newCameraMatrixR, distR, grayL.shape[::-1], rot, trans, rectifyScale, (0,0))

        stereoMapL = cv2.initUndistortRectifyMap(newCameraMatrixL, distL, rectL, projMatrixL, grayL.shape[::-1], cv2.CV_16SC2)
        stereoMapR = cv2.initUndistortRectifyMap(newCameraMatrixR, distR, rectR, projMatrixR, grayR.shape[::-1], cv2.CV_16SC2)

        # save camera parameters to .xml file
        print('Saving camera parameters to file...')
        cv_file = cv2.FileStorage('stereoMap.xml', cv2.FILE_STORAGE_WRITE)

        cv_file.write('stereoMapL_x', stereoMapL[0])
        cv_file.write('stereoMapL_y', stereoMapL[1])
        cv_file.write('stereoMapR_x', stereoMapR[0])
        cv_file.write('stereoMapR_y', stereoMapR[1])

        cv_file.release()
        print('Calibration Complete!\n')

        return True
    
    
    def Calculate3DCoordiantes(img_left, img_right, coordL, coordR):
        '''
        Summary:
        Function is used to calculate the depth coordinate value from
        the stereo images and returns a string containing all of the
        joint coordinates for the image frame.

        Parameters:\n
        img_* >> the path to the image including the file name (../../media/tracking/img_#.png)\n
        coord* >> the coordinate string returned from openpose

        Return:\n
        coordinate_output >> the coordinate string containing all the 3D joint coordinates
        '''
        frame_rate = 30 #camera frame rate
        B = 10          #distance between the cameras [cm]
        f = 8           #Camera lens' focal length [mm]
        fov = 60        #camera field of view in the horizontal plane [degrees]
        print("Calculating 3D coordinates")

        # read the stored images
        imgL = cv2.imread(img_left)
        imgR = cv2.imread(img_right)

        frame_left, frame_right = ImRec.undistortRectify(imgL, imgR)

        coordL_array = HLCameraCalibration.convert_string_to_npArray(coordL)
        coordR_array = HLCameraCalibration.convert_string_to_npArray(coordR)

        coordinate_output = '[['
        for i in range(25):

            pointL = coordL_array[i]
            xL, yL = pointL
            calc_p_L = (xL, yL)

            pointR = coordR_array[i]
            xR, yR = pointR
            calc_p_R = (xR, yR)

            depth = tri.find_depth(calc_p_L, calc_p_R, frame_left, frame_right, B, f, fov)

            coordinate_output += f'[{xL} {yL} {depth}]'

        coordinate_output += ']]'
        return coordinate_output

    
    def convert_string_to_npArray(OP_Coord_String):
        '''
        Summary:
        Function takes in an openpose coordinate string and converts it
        into a numpy array of (x, y) coordinates for depth calculation.

        Parameters:\n
        OP_Coord_String >> an openpose coordinate string

        Return:\n
        np.array() >> returns a numpy array
        '''

        # Remove the brackets and new line characters
        float_vals = OP_Coord_String.replace("[", "").replace("]", "").replace("\n", "")
        # Split the string into a list of strings
        float_list = float_vals.split()
        # Convert the list of strings to a list of floats
        float_list = [float(i) if i is not None else 0 for i in float_list]
        # Convert the list to a numpy array
        np_coord_array = np.array(float_list)
        # Reshape the array to 2D with 3 columns (X, Y, Z)
        np_coord_array = np_coord_array.reshape(-1, 3)
        # Remove the Z coordinate (third column)
        np_coord_array = np_coord_array[:, :2]
        # Ensure the array is continuous
        np_coord_array = np.ascontiguousarray(np_coord_array)
        # Ensure the array has the correct depth
        return np_coord_array.astype(np.float32)
