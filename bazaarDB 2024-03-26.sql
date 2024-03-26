-- phpMyAdmin SQL Dump
-- version 5.2.1
-- https://www.phpmyadmin.net/
--
-- Host: db
-- Generation Time: Mar 26, 2024 at 09:16 AM
-- Server version: 5.7.42
-- PHP Version: 8.1.18

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Database: `bazaarDB`
--

-- --------------------------------------------------------

--
-- Table structure for table `Builds`
--

CREATE TABLE `Builds` (
  `buildID` bigint(20) UNSIGNED NOT NULL,
  `buildName` varchar(100) CHARACTER SET ascii COLLATE ascii_bin NOT NULL,
  `userID` bigint(20) UNSIGNED NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `BuildUrls`
--

CREATE TABLE `BuildUrls` (
  `buildUrlID` bigint(20) UNSIGNED NOT NULL,
  `buildID` bigint(20) UNSIGNED NOT NULL,
  `buildUrl` varchar(300) CHARACTER SET ascii COLLATE ascii_bin NOT NULL,
  `buildUrlName` varchar(100) CHARACTER SET ascii COLLATE ascii_bin NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `Images`
--

CREATE TABLE `Images` (
  `imageID` bigint(20) UNSIGNED NOT NULL,
  `filePath` varchar(300) CHARACTER SET ascii COLLATE ascii_bin NOT NULL,
  `typeID` bigint(20) UNSIGNED NOT NULL,
  `buildID` bigint(20) UNSIGNED NOT NULL,
  `userID` bigint(20) UNSIGNED NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `ImageTypes`
--

CREATE TABLE `ImageTypes` (
  `typeID` bigint(20) UNSIGNED NOT NULL,
  `typeName` varchar(100) CHARACTER SET ascii COLLATE ascii_bin NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

--
-- Dumping data for table `ImageTypes`
--

INSERT INTO `ImageTypes` (`typeID`, `typeName`) VALUES
(1, 'Build Tumbnail'),
(2, 'Reference Image');

-- --------------------------------------------------------

--
-- Table structure for table `Notes`
--

CREATE TABLE `Notes` (
  `noteID` bigint(20) UNSIGNED NOT NULL,
  `buildID` bigint(20) UNSIGNED NOT NULL,
  `filePath` varchar(300) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `Users`
--

CREATE TABLE `Users` (
  `userID` bigint(20) UNSIGNED NOT NULL,
  `userName` varchar(20) CHARACTER SET ascii COLLATE ascii_bin NOT NULL,
  `email` varchar(100) CHARACTER SET ascii COLLATE ascii_bin NOT NULL,
  `password` varchar(100) CHARACTER SET ascii COLLATE ascii_bin NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

--
-- Indexes for dumped tables
--

--
-- Indexes for table `Builds`
--
ALTER TABLE `Builds`
  ADD PRIMARY KEY (`buildID`),
  ADD KEY `userID` (`userID`);

--
-- Indexes for table `BuildUrls`
--
ALTER TABLE `BuildUrls`
  ADD PRIMARY KEY (`buildUrlID`),
  ADD KEY `FK_BuildUrlBuildID` (`buildID`);

--
-- Indexes for table `Images`
--
ALTER TABLE `Images`
  ADD PRIMARY KEY (`imageID`),
  ADD KEY `FK_ImagesBuildID` (`buildID`),
  ADD KEY `FK_ImagesTypeID` (`typeID`),
  ADD KEY `FK_ImagesUserID` (`userID`);

--
-- Indexes for table `ImageTypes`
--
ALTER TABLE `ImageTypes`
  ADD PRIMARY KEY (`typeID`);

--
-- Indexes for table `Notes`
--
ALTER TABLE `Notes`
  ADD PRIMARY KEY (`noteID`),
  ADD KEY `FK_NotesBuildID` (`buildID`);

--
-- Indexes for table `Users`
--
ALTER TABLE `Users`
  ADD PRIMARY KEY (`userID`);

--
-- AUTO_INCREMENT for dumped tables
--

--
-- AUTO_INCREMENT for table `Builds`
--
ALTER TABLE `Builds`
  MODIFY `buildID` bigint(20) UNSIGNED NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `BuildUrls`
--
ALTER TABLE `BuildUrls`
  MODIFY `buildUrlID` bigint(20) UNSIGNED NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `Images`
--
ALTER TABLE `Images`
  MODIFY `imageID` bigint(20) UNSIGNED NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `ImageTypes`
--
ALTER TABLE `ImageTypes`
  MODIFY `typeID` bigint(20) UNSIGNED NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=3;

--
-- AUTO_INCREMENT for table `Notes`
--
ALTER TABLE `Notes`
  MODIFY `noteID` bigint(20) UNSIGNED NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `Users`
--
ALTER TABLE `Users`
  MODIFY `userID` bigint(20) UNSIGNED NOT NULL AUTO_INCREMENT;

--
-- Constraints for dumped tables
--

--
-- Constraints for table `Builds`
--
ALTER TABLE `Builds`
  ADD CONSTRAINT `Builds_ibfk_1` FOREIGN KEY (`userID`) REFERENCES `Users` (`userID`);

--
-- Constraints for table `Images`
--
ALTER TABLE `Images`
  ADD CONSTRAINT `FK_ImagesBuildID` FOREIGN KEY (`buildID`) REFERENCES `Builds` (`buildID`),
  ADD CONSTRAINT `FK_ImagesTypeID` FOREIGN KEY (`typeID`) REFERENCES `ImageTypes` (`typeID`),
  ADD CONSTRAINT `FK_ImagesUserID` FOREIGN KEY (`userID`) REFERENCES `Users` (`userID`);

--
-- Constraints for table `Notes`
--
ALTER TABLE `Notes`
  ADD CONSTRAINT `FK_NotesBuildID` FOREIGN KEY (`buildID`) REFERENCES `Builds` (`buildID`);
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
