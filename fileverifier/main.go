package main

import (
	"crypto/md5"
	"encoding/json"
	"fmt"
	"io"
	"net/http"
	"os"
)

var completed = make(chan Software, 100)

type ProgramSeason struct {
	Year     int        `json:"Year,omitempty"`
	Program  string     `json:"Program,omitempty"`
	Software []Software `json:"Software,omitempty"`
}
type Software struct {
	Name        string   `json:"Name,omitempty"`
	Description string   `json:"Description,omitempty"`
	Tags        []string `json:"Tags,omitempty"`
	FileName    any      `json:"FileName,omitempty"`
	URI         string   `json:"Uri,omitempty"`      // String
	Hash        any      `json:"Hash,omitempty"`     // String or null
	Platform    any      `json:"Platform,omitempty"` // String or null
}

func (s *Software) Download() error {
	if s.URI == "" {
		return fmt.Errorf("URI is empty")
	}
	if s.Hash == nil {
		return nil
	}
	fmt.Println("Downloading", s.Name)
	client := http.DefaultClient
	resp, err := client.Get(s.URI)
	if err != nil {
		return err
	}
	destFile, err := os.CreateTemp("", s.Name)
	if err != nil {
		return err
	}
	defer destFile.Close()

	hasher := md5.New()
	defer hasher.Reset()
	n, err := io.Copy(destFile, resp.Body)
	if err != nil {
		return err
	}
	fmt.Printf("Downloaded %s to %s (%d bytes)\n", s.Name, destFile.Name(), n)

	// Calculate the hash
	hash := fmt.Sprintf("%x", hasher.Sum(nil))
	if s.Hash != hash {
		return fmt.Errorf("%s: hash mismatch - expected: %s != %s", s.URI, s.Hash, hash)
	}
	return nil
}

func main() {
	args := os.Args
	if len(args) > 1 {
		filePath := args[1]
		_, err := os.Stat(filePath)
		if os.IsNotExist(err) {
			fmt.Println("File does not exist")
			os.Exit(1)
		}
		if err := checkFile(filePath); err != nil {
			fmt.Println(err)
			os.Exit(1)
		}
	} else {
		fmt.Println("Please provide a file path")
		os.Exit(1)
	}
}

func checkFile(filePath string) (err error) {
	// Open the file
	file, err := os.Open(filePath)
	if err != nil {
		fmt.Println(err)
		return err
	}
	err = file.Close()
	if err != nil {
		return err
	}
	rawBytes, err := os.ReadFile(filePath)

	if err != nil {
		fmt.Println(err)
		return err
	}
	// Decode the JSON
	var programSeason ProgramSeason
	err = json.Unmarshal(rawBytes, &programSeason)
	if err != nil {
		fmt.Println(err)
		return err
	}

	// Download the software
	for _, software := range programSeason.Software {
		go func() {
			err := software.Download()
			if err != nil {
				fmt.Println(err)
			} else {
				fmt.Printf("Downloaded %s successfully\n", software.Name)
				completed <- software
			}
		}()
	}

	completedCount := 0
	for {
		select {
		case software := <-completed:
			completedCount++
			fmt.Printf("Downloaded %s successfully\n", software.Name)
		}
		if completedCount == len(programSeason.Software) {
			fmt.Println("All downloads completed successfully")
			break
		}
	}
	return nil
}
